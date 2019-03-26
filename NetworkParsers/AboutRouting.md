# About Routing in general
Routing is how a programmers lets a web framework know which function to call when various 
URLs are sent to the framework. For example, when an end-user does a GET to /mypage, 
the routing framework will figure out which function handles a 'get' to /mypage.

Every web framework includes some kind of router!

## PHP Lumen
The  [Lumen](https://lumen.laravel.com/docs/5.4/routing) PHP framework syntax looks like this:

```PHP
$app->get('mypage', function () {
    return 'Hello World';
});
```

There are specializations for get, post, put, patch, delete and options.

Parameters are like this:

```PHP
$app->get('posts/{postId}/comments/{commentId}', function ($postId, $commentId) {
    //
});
```

Fancy features
1. The {id} can include regular expressions
2. There is a naming thing I do not understand.
3. There are route groups
4. You can prefix routes

## Ruby on Rails
I used the [Ruby on Rails](https://guides.rubyonrails.org/routing.html) guide

Example of a simple route. The request will go to the patient controller show function with 
a dictionary with id=... as a parameter.

```ruby
get '/patients/:id', to: 'patients#show'
```

## WCF Routing
Looking at the [System.Web.Routing](https://docs.microsoft.com/en-us/dotnet/api/system.web.routing.route?view=netframework-4.7.2) 
objects 

```csharp
void Application_Start(object sender, EventArgs e) 
{
    RegisterRoutes(RouteTable.Routes);
}

public static void RegisterRoutes(RouteCollection routes)
{
    routes.Add(new Route
    (
         "Category/{action}/{categoryName}"
         , new CategoryRouteHandler()
    ));
}
```

The CategoryRouteHandler is an [IRouteHandler](https://docs.microsoft.com/en-us/dotnet/api/system.web.routing.iroutehandler.gethttphandler?view=netframework-4.7.2) 
with a method GetHttpHandler (RequestContext); it looks like the developer can add in multiple layers of indirection.

## Java Play framework
Looking at an [introduction](https://www.baeldung.com/routing-in-play) to Play, which looks like a simple 
Java framework (and might be the Spring framework?). All of the routing is done via a configuration file.

```
GET     /     controllers.HomeController.index
GET     /     count controllers.CountController.count
GET     /     message controllers.AsyncController.message
 
GET     /     assets/*file controllers.Assets.versioned(path="/public", file: Asset)
```