## Unity_DevConsole
개발용 콘솔창

# Input
| platform | key |
| ------ | ------ |
| window open/close | ` |
| app open/clsoe | 4touch |

# Command
- 사용할수있는 커멘드 출력
```sh
help
```

# Command 추가

- DevCONCommand.cs
커멘드 클래스 추가
```sh
CDevCommandNewClass : IDevCommand
{
    public string GetID => "command id";
    public void Run( string[] datas/*value*/ ) { /*...*/ }
}
```

- Register CommandClass
```sh
CDevCONCommand.RegisterCommand( new CDevCommandNewClass () );
```

- Register Delegate
```sh
CDevCONCommand.RegisterCommand( "ItemOn"/*command id*/, cmdFun /*UnityAction<string[]>*/ );
```
