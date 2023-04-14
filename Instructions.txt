These are instructions for how to create unit tests and fix the current failing test. In addition there are steps to add a fourth unit test.

*Working Unit Test*
Currently there are three unit tests in the Software_Testing_Training/WingtipToys.Tests two are working and one is failing. To start with we will look at the passing test named ShoppingCartTest_NewItemAddedtoDB.

Unit Tests are generally separated into three sections: Setup, Action, Assert. The Setup portion of the test is creating objects and variables that are needed for the action step. The Action step is where the system under test is called. This can be an entire class, a specific method, or even a just a piece of logic inside of a method. Lastly, Assert is where validation takes place. This is where the system under test is checked to ensure it behaved as expected.

Generally, it is best to work backwards. Starting with Assert, figuring out what a successful test run will look like. For the passing unit test (ShoppingCartTest_NewItemAddedtoDB), a successful test run will be one where at the end there is a new item added to the database for a new cart. So the assertion step gets the CartItem from the database and then interrogates it to validate that it was properly created.

Working backwards, lets take a look at the action, it creates the system under test which is the ShoppingCartActions class and then calls the AddToCart() method. Looking at that class and method a database will need to be create and passed in and a ItemId is needed. These are the things that need to be created in the setup phase.

Speaking of the setup phase, this phase generally has the most in it. For our test we need a database, an HttpContext, as well as some variables. Instead of duplicating code across multiple tests, helpers were created for different tests to use. In addition to the database created inside of the test, a HttpContext is created in the TestInitalize step. TestInitalize is run before each test. If there are things that every test will need those steps can be placed it the TestInitalize method. The database creation could likely go in there but the two tests need different things in their database.

In summary, this test creates all of the items it needs and then calls shoppingCartAction.AddToCart(ProductId); in that method it is checked to see if there is a shoppingCart that already exists, in this case there is not. It will then create the cart and cartItem to then be saved in the database. The test then queries the database to validate that the item has been successfully saved in the database.

*Failing Test*

 Skipping the suspense, the reason the test is failing is because the productId mismatches. So a new item is being added to the cart instead of increasing the quantity of the item already in the cart.

This could easily happen if someone writing multiple tests was copying and pasting from other tests to create a new test. This does demonstrate that sometime the test is broken and needs to be fixed. Below is an example of writing a test that exposes an issue with the code and finds a bug.

*Writing your own test*

The third unit test ShoppingCartTest_TotalDoesNotGetsDiscountAppliedOnOrderUnder20 is testing that a discount is not applied when the carts value is less than $20. There should be a second unit test that tests if the opposite works as well.

The goal of the unit test is to check that a cart with a value equal or greater than $20 gets the 15% discount applied. There is another helper method for this test that creates products and takes in the price of a product as a input.

The first thing to think about is naming the unit test. It should be something descriptive such as ShoppingCartTest_TotalHasDiscountAppliedOnOrderOver20. It should also have [TestMethod] on the line prior to the title. Much of the test can be copied from the other test.

There is a variable for the UnitPrice. Changing that value to be greater than $20 and running the test will hopefully cause the test to fail because the UnitPrice and the Result are two different values. The result will have been reduced by 15%. Change the expected value to DiscountedUnitPrice and add a line above the assertion that is var DiscountedUnitPrice = UnitPrice * .85;

The test should now pass.

However there is a bug in this code. What happens if a value of exactly $20 is entered? Given the expected behavior is the discount is applied to an total of $20 the we would expect the total to be $17 but instead we get a value of $20. This is because the logic in the get total method is wrong. If this were a real project filing a bug would be appropriate. However, in this scenario it can be ignored or go to ShoppingCartActions.cs and change line 111 to if (total >= 20)
