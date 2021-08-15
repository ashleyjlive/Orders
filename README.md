# Orders

## Design Requirements

As per the design requirements, an application is required for generating sales
reciepts for customer orders. This, should be in the form of a WebAPI.

Notable points:
- An order can include multiple products.
- A product has a title, description and a price.
- A product can have discounts applied if multiple products of the same type are ordered, product discounts can often change.
- A sales tax is added to the final order total.
- A sales receipt should detail:
    - The date of the order.
    - A list of the products ordered.
    - Any discounts applied to products.
    - The subtotal of the order (pre-tax).
    - The sales tax applied to the order.
    - The total order price.
    - The total of discounts applied.
- HTML and Text versions of the sales receipt can be requested.

### Technology
The required technology for this task is the utilisation of .NET WebAPI.

To store the data used for generating the receipt, a SQLite database will be 
used (however this can be swapped out for any database using EF).

## Notable Considerations
It is important that changes in product pricing or tax DOES NOT affect already
existing orders. Because of this, such data must be stored at the time of the 
order being created in the instance that the receipt is generated at a later 
point in time.

## Design
We will now define the classes which will represent our data.
These will be located in the `Models` directory.

### Product
The products definition will define:
- The ID of the product.
- The title of the product.
- The description of the product.
- The price of the product.

### Order
The orders definition will define:
- The ID of the order.
- The time of the order.
- A list of product orders (essentially a list of products and quantities).
- The tax that is to be applied to the order.
- The subtotal.
- The total of the order.

### Product Discount
The product discount definition will define:
- The ID of the product discount (mainly for internal use).
- The ID of the product that is to be discounted.
- The discount to be applied to the product.
- The threshold of the discount (the minimum number of products before the discount applies).

### Product Order
The product order definition will define:
- The id of the product order (mainly for internal use).
- The order id that this relates to.
- The product id that was requested in the order.
- The product price at the time of the order (in case prices update).
- The discount of the product at the time of the order (in case discounts change).
- The quantity.
- The sum of the discount.
- The total of this product order.

## Persistance
There will be three interfaces, `IOrderStorage`, `IProductStorage` and 
`IProductDiscountStorage`. These will all define the interfaces to perform CRUD 
operations on the database.
<br>
Each implementation will be named as `OrderStorage`, `ProductStorage` and 
`ProductDiscountStorage` respectively, each accepting a `DataContext`. 
This `DataContext` is a `DbContext` which is defined in `Startup.cs`. 
As a result of this, switching of database implementations without rewriting 
code is possible.

## Requests
All API request controllers will be defined under `Controllers`. 
These controllers are:
- DiscountsController
- OrdersController
- ProductsController

## Formatters
When the `OrdersController` returns an order and the request accepts a content 
type such as `text/plain` or `text/html` then the `OrderFormatter` will provide 
a custom implementation for rendering a simplistic view of an order in either
HTML or Plain Text.

## Formatting
### Text
The desired output when requesting text output should be as follows:<br><br>
Order ID: 71c21541-e797-4374-a0a2-7495878299e6<br>
Time: 11/01/1999 10:20:45pm<br>
<br>
Products:<br>
• Product 1 (2x) - $10.99 each at 10% off<br>
• Product 2 (3x) - $1.10 each at 5% off<br>
<br>
Total savings: $2.36<br>
Tax: $2.30<br>
Sub Total: $22.92<br>
Total: $25.22

### HTML
If the same order as above is requested in HTML format then the output would 
look like the following:
```
<html>
<body>
    <h1>Order Receipt</h1>
    <h3>Order ID: 71c21541-e797-4374-a0a2-7495878299e6</h3>
    <h3>Order Time: 11/01/1999 10:20:45pm</h3>
    <h3>Products Ordered</h3>
    <table style="width:30%">
        <tr>
            <th>Name</th>
            <th>Product Price</th>
            <th>Quantity</th>
            <th>Discount</th>
            <th>Sum</th>
        </tr>
        <tr>
            <td>Product 1</th>
            <td>$10.99</th>
            <td>2</th>
            <td>10%</th>
            <td>$19.78</th>
        </tr>
        <tr>
            <td>Product 2</th>
            <td>$1.10</th>
            <td>3</th>
            <td>5%</th>
            <td>$3.13</th>
        </tr>
    </table>
    <h3>Total savings: $2.36</h3>
    <h3>Tax: $2.30</h3>
    <h3>Subtotal: $22.92</h3>
    <h3>Total: $25.22</h3>
<body>
</html>
```
Which gives the appearance of:
<html>
<body>
    <h1>Order Receipt</h1>
    <h3>Order ID: 71c21541-e797-4374-a0a2-7495878299e6</h3>
    <h3>Order Time: 11/01/1999 10:20:45pm</h3>
    <h3>Products Ordered</h3>
    <table style="width:30%">
        <tr>
            <th>Name</th>
            <th>Product Price</th>
            <th>Quantity</th>
            <th>Discount</th>
            <th>Sum</th>
        </tr>
        <tr>
            <td>Product 1</th>
            <td>$10.99</th>
            <td>2</th>
            <td>10%</th>
            <td>$19.78</th>
        </tr>
        <tr>
            <td>Product 2</th>
            <td>$1.10</th>
            <td>3</th>
            <td>5%</th>
            <td>$3.13</th>
        </tr>
    </table>
    <h3>Total savings: $2.36</h3>
    <h3>Tax: $2.30</h3>
    <h3>Subtotal: $22.92</h3>
    <h3>Total: $25.22</h3>
<body>
</html>
<br>

## API Design
This API implements basic functionality in order to create orders.

### `/products` PUT Endpoint
This endpoint will allow for a product to be created.
It will accept JSON data of the form:
```
{
    "title": "Some title here",
    "description": "Some description here",
    "price": 50.00
}
```
The response will simply be a HTTP 200 with the ID of the newly created product.

### `/products/{id}` GET Endpoint
This retrieves the product details from its ID. The response will be in the format of:
```
{
    "id": "an id here"
    "title": "Some title here",
    "description": "Some description here",
    "price": 50.00
}
```

### `/products` POST Endpoint
This upserts a product. If a product already exists with the ID, then the data is updated, if not, the product is added to the database.
```
{
    "id": "an id here"
    "title": "Some title here",
    "description": "Some description here",
    "price": 50.00
}
```

### `/products/{id}` DELETE Endpoint
This deletes a endpoint for a given product ID.
If the request finds a product with the provided ID and successfully deletes it a HTTP 200 is returned.
If the request does not find an association, a HTTP 404 is returned.

### `/discounts` PUT Endpoint
This endpoint will allow for a discount to be associated with a product.
It will accept JSON data of the form:
```
{
    "discount": 0.1,
    "productId": "productId here",
    "threshold": 2
}
```
The response will simply be a HTTP 200.

### `/discounts/{productId}` GET Endpoint
This endpoint will allow for a discount for an associated product id to be retrieved.
The response will be in the format of:
```
{
    "discount": 0.1,
    "productId": "product id here",
    "threshold": 2
}
```

### `/discounts/{productId}` DELETE Endpoint
This deletes a discount for a given product ID.
If the request finds a discount and successfully deletes it a HTTP 200 is returned.
If the request does not find an association, a HTTP 404 is returned.

### `/orders` PUT Endpoint
This will create a new order.
Simply, the accepted JSON format is as follows:
```
{
  "productOrders": {
    "product_id_1": 50,
    "product_id_2": 1,
    "product_id_3": 34
  }
}
```
The integer values in the request represent the quantity of each product.
The response will be a HTTP 200 with the ID of the order.

### `/orders/{id}` GET Endpoint
This will return all details about a given order. It supports the following display types:
- text/plain
- text/html
- text/json
- application/json

If the order does not exist, a HTTP 404 is returned.

### `/orders/{id}` DELETE Endpoint
This deletes a order for a given order ID.
If the request finds a order with the provided ID and successfully deletes it a HTTP 200 is returned.
If the request does not find an association, a HTTP 404 is returned.

## Limitations
### Customers
For the purposes of this task, no mention of customer details are present. In a real scenario, customer details must also be recorded.
### Templating
Also, to make updating templates (particularly HTML) easier and more reliable in future, it is suggested to use Razor pages as these are specifically designed for templating dynamic data.
### Space Considerations
Because of the duplication of data fields such as price, there is concerns with respect to disk usage and database size over time.