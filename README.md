# Nitrolize
Nitrolize accelerates your GraphQL server development in C#.

Based on [GraphQL for .NET](http://github.com/graphql-dotnet/graphql-dotnet) it offers
* mechanisms to auto generate GraphQL Types from your domain model classes,
* auto converts Guid / int Ids from your domain model to Base64 encoded unique ids for your client and vice versa,
* simplifies field declaration by an easy to use syntax.

## Example usage
Your user domain model:
```csharp
public class User
{
    public Guid Id { get;set; }
    public string NickName { get; set; }
    public string Mail { get; set; }
    public int FavoriteNumber { get; set; }
}
```
Your ViewerType:
```csharp
public class ViewerType : NitrolizeViewerType
{
    private readonly IUserRepository userRepository;
    
    public ViewerType(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    /// <summary>
    /// Gets a user by id.
    /// </summary>
    [Field]
    public Field<User, Guid> User => (context, id) =>
    {
        return this.userRepository.GetUserById(id);
    };
        
    /// <summary>
    /// Gets all users.
    /// </summary>
    [Connection]
    public ConnectionField<User> Users => (context, parameters) =>
    {
        return new Connection<User, Guid>(this.userRepository.GetAllUsers());
    };
}
```

Possible GraphQL query:
```
{
  viewer {
    users {
      edges {
        cursor
        node {
          id
          nickName
          mail
          favoriteNumber
        }
      }
    }
    user(id: "VXNlciNjYWVmYjc5Mi04ODFmLTRmMjAtYmI5ZC1jNDAzMjY5OGQxMGM=") {
      nickName
      mail
      favoriteNumber
    }
  }
}
```
