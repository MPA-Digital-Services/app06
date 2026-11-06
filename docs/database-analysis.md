# Análisis de la base de datos Northwind

Documento de referencia del esquema cargado desde `instnwnd.sql`, de las decisiones de mapeo
en EF Core y de los puntos delicados encontrados durante la implementación.

---

## 1. Resumen del esquema

| Objeto            | Cantidad |
|-------------------|----------|
| Tablas            | 13       |
| Vistas            | 16       |
| Procedimientos    | 7        |
| Funciones (user)  | 0        |
| Diagramas         | 1 (`Orders`) |

### 1.1 Tablas y filas (tras cargar `instnwnd.sql`)

| Tabla                    | Filas | Notas |
|--------------------------|------:|-------|
| `Categories`             | 8     | `CategoryID` IDENTITY; `Picture` es `image`. |
| `CustomerCustomerDemo`   | 0     | Tabla puente (Customer ↔ CustomerDemographics). |
| `CustomerDemographics`   | 0     | `CustomerTypeID` PK string, sin datos en el script. |
| `Customers`              | 91    | `CustomerID` es `nchar(5)` (clave de negocio, no IDENTITY). |
| `Employees`              | 9     | Autorreferencia `ReportsTo`; `Photo` es `image`. |
| `EmployeeTerritories`    | 49    | Tabla puente (Employee ↔ Territories). |
| `Order Details`          | 2155  | Clave primaria compuesta `(OrderID, ProductID)`; `Discount` `float`. |
| `Orders`                 | 830   | FK a Customers, Employees, Shippers. |
| `Products`               | 77    | FK a Categories y Suppliers. |
| `Region`                 | 4     | `RegionID` **no** IDENTITY; PK no agrupada. |
| `Shippers`               | 3     | `ShipperID` IDENTITY. |
| `Suppliers`              | 29    | `HomePage` es `ntext`. |
| `Territories`            | 53    | `TerritoryID` PK string; FK a `Region`. |

### 1.2 Vistas (todas de solo lectura)

Customer and Suppliers by City · Alphabetical list of products · Current Product List · Orders Qry ·
Products Above Average Price · Products by Category · Quarterly Orders · Invoices ·
Order Details Extended · Order Subtotals · Product Sales for 1997 · Category Sales for 1997 ·
Sales by Category · Sales Totals by Amount · Summary of Sales by Quarter · Summary of Sales by Year.

### 1.3 Procedimientos almacenados

| Procedimiento | Parámetros | Resultado |
|---------------|-----------|-----------|
| `Ten Most Expensive Products` | — | Producto y precio (usa `SET ROWCOUNT`) |
| `Employee Sales by Country` | `@Beginning_Date`, `@Ending_Date` | Ventas por empleado |
| `Sales by Year` | `@Beginning_Date`, `@Ending_Date` | Subtotales por año |
| `CustOrdersDetail` | `@OrderID` | Detalle de líneas del pedido |
| `CustOrdersOrders` | `@CustomerID` | Pedidos del cliente |
| `CustOrderHist` | `@CustomerID` | Cantidades por producto del cliente |
| `SalesByCategory` | `@CategoryName`, `@OrdYear` | Ventas por producto (1996/1997/1998) |

---

## 2. Relaciones

```
Categories 1───* Products *───1 Suppliers
Customers  1───* Orders    *───1 Employees (ReportsTo → Employees)
Shippers   1───* Orders
Orders     1───* Order Details *───1 Products
Customers  *───* CustomerDemographics   (CustomerCustomerDemo)
Employees  *───* Territories            (EmployeeTerritories)
Region     1───* Territories
```

---

## 3. Mapeo en EF Core (database-first)

- Scaffolding con `--use-database-names --no-onconfiguring`, contexto y entidades en
  `src/NorthwindApi.Infrastructure/Data/Generated`.
- Las **vistas** se generan como entidades **keyless**: `entity.HasNoKey().ToView("...")`.
- Las **tablas puente** `CustomerCustomerDemo` y `EmployeeTerritories` **no** generan entidad: se
  modelan como navegaciones de muchos-a-muchos (`UsingEntity<Dictionary<string,object>>`).
- El contexto es **regenerable**; no debe editarse a mano. Las personalizaciones van en `OnModelCreatingPartial`.

### 3.1 Precisión de tipos (origen de varios errores en tiempo de ejecución)

| Columna SQL      | Tipo .NET | Punto sensible |
|------------------|-----------|----------------|
| `Discount`       | `float` (`real`) | Operaciones aritméticas requieren cast a `decimal` para sumar importes. |
| `Quantity`       | `short` (`smallint`) | Un DTO que lo declare `int` falla con `InvalidCastException` (Int16→Int32) al leer procedimientos/vistas. |
| `UnitPrice`, `Subtotal`, `ExtendedPrice`, `Freight` | `decimal` (`money`) | Sumas monetarias. |
| `CustomerID`     | `string` `nchar(5)` | Clave de negocio; longitud fija. |
| `UnitsInStock`, `UnitsOnOrder`, `ReorderLevel` | `short?` | Nulables. |

> **Importante:** `CustOrdersDetail` devuelve `Quantity` como `smallint`; el DTO usa `short?`.
> Declararlo `int?` provoca un `InvalidCastException` en tiempo de ejecución.

---

## 4. Reglas de negocio derivadas

- **No hay tabla de pagos/cobros.** “Ventas” = `SUM(UnitPrice * Quantity * (1 - Discount))` sobre
  `Order Details` (unidas a `Orders` cuando se necesita `OrderDate`).
- **No hay columna de estado de pedido.** Se deriva de `ShippedDate`/`RequiredDate`:
  `Enviado` (tiene `ShippedDate`), `Vencido` (sin enviar y `RequiredDate` en el pasado), `Pendiente` (resto).
- **Filtro temporal** de métricas por `Orders.OrderDate`.

---

## 5. Restricciones e integridad

- **Claves foráneas** → número de error SQL **547** (mapeado a **409**).
- **Duplicados de clave/único** → **2601/2627** (mapeado a **409**).
- **Nulos en columnas no nulables** → **515** (400); **longitud excedida** → **2628/8152** (400).
- `CK_Birthdate` en `Employees` exige `BirthDate < GETDATE()` (validado también en la capa de aplicación).
- `Region.RegionID` **no** es IDENTITY: el cliente debe proporcionar el identificador en la creación.

---

## 6. Observaciones y rarezas

- `Ten Most Expensive Products` usa `SET ROWCOUNT`, hoy obsoleto; se conserva tal cual el script original.
- Varias vistas de ventas están **fijadas a 1997** (“… for 1997”) y `SalesByCategory` varía según `@OrdYear`.
- `FlowControl`/`Diagrams`: la tabla `sysdiagrams` puede existir si se regeneró el diagrama; es irrelevante para la API.
- Las tablas `CustomerDemographics` y `CustomerCustomerDemo` están **vacías** en los datos de ejemplo; no hay
  rutas CRUD para la relación muchos-a-muchos (sí para `CustomerDemographics` como entidad).
- Los importes `money` conservan 4 decimales en el origen; la API los expone como `decimal`.

---

## 7. Regeneración del scaffolding (referencia)

```powershell
dotnet ef dbcontext scaffold `
  "Server=(localdb)\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --context NorthwindContext `
  --context-dir Data/Generated `
  --output-dir Data/Generated `
  --use-database-names `
  --no-onconfiguring `
  --force `
  --project src/NorthwindApi.Infrastructure
```

> Tras regenerar, comprobar que los nombres de entidades/DbSets coinciden con los usados por la capa de
> infraestructura (servicios y reportes).
