# dotnet-guzzle
This is a wrapper library for performing http requests in .NET


### Implementation
```C#
using Cyberliberty.Guzzle;
```

### Usage
```C#

/* initialize new instance */
Guzzle guzzle = new Guzzle( "http://www.example.com/" );

/* initialize new instance with extra options */
Guzzle guzzle = new Guzzle() {
    Url = "http://www.example.com/",
    AllowAutoRedirect = true,
    UseProxy = true,
    Proxy = "https://proxy.example.com:8873",
};

```

### Add Headers / Files / Json
```C#

/* headers */
guzzle.AddHeader("User-Agent", "Cyberliberty.Guzzle (Guzzle v1.0.1 Beta)");
guzzle.AddHeader("Authorization", "token 098f6bcd4621d373cade4e832627b4f6");
guzzle.AddHeader("X-Forwared-For", "127.0.0.1");

/* json body */
guzzle.AddJson("username", "admin");
guzzle.AddJson("password", "********");

/* form-data body */
guzzle.AddParam("username", "admin");
guzzle.AddParam("password", "********");

/* upload files */
guzzle.AddFile("some_file", @"C:\Users\John\Desktop\profile.png");

/* Raw body */
guzzle.RawBody("username=admin&password=********&login=submit");
```

### Synchronous call
```C#
/* call and get response */
var respnonse = guzzle.Head();
var respnonse = guzzle.Get();
var respnonse = guzzle.Post();
```

### Asynchronous call
```C#
/* call and get response */
var respnonse = await guzzle.HeadAsync();
var respnonse = await guzzle.GetAsync();
var respnonse = await guzzle.PostAsync();
```

### Asynchronous call with callback
```C#
/* call and get response */
guzzle.HeadAsync(response => {});
guzzle.GetAsync(response => {});
guzzle.PostAsync(response => {});
```

### Response object
```C#
/**
* Response {
*     [int] Status
*     [string] Url
*     [string] Method
*     [string] HttpVersion
*     [Dictionary<string, string>] Headers
*     [string] Content
* }
*/
```
