# Turnit Store

## General issues

There are some performance and maintainability problems with the current solution.

Firstly, client has reported that the applications memory consumption grows very fast.
Also the code performance and maintainability is not so good - code duplication and bad code.

## Assignments

You are free to use any additional libraries, patterns, etc that you find fit (except replacing NHibernate). Database
changes are not required.

### Task 1 (done)

Add functionality to add/remove products to/from categories.

Implement methods:

* `PUT /products/{productId}/category/{categoryId}`
* `DELETE /products/{productId}/category/{categoryId}`

### Task 2

Add functionality to change products availability in different stores.

Implement methods `POST /products/{productId}/book` and `POST /store/{storeId}/restock`

### Task 3 (done)

Find and fix the memory leak issue.
Improve the overall code quality in `GET /products/*` methods.