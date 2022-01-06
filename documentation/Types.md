## (De-)Serializable Types

### "Simple Values"

Simple values are that can be described as a "simple" string.  
That means Primitive types, Enums, Strings.  
This also includes custom Types that have a dedicated TypeConverter (see MS-Docs `TypeConverterAttribute`)

Example:

```c#
enum ExampleEnum {
    Value1,
    Value2
}

class ConfigClass {
    [ConfigValue] int primitiveValue = 123;
    [ConfigValue] ExampleEnum enumValue = ExampleEnum.Value1;
    [ConfigValue] string stringValue = "someString";
}
```
```
- primitiveValue = 123
- enumValue = Value1
- stringValue = someString
```

---

### List\<T\>

A `System.Collections.Generic.List<T>` list of instances of a Type T.  
`T` can be any (de-)serializable type.

Example:

```c#
class ConfigClass {
    [ConfigValue] List<int> simpleTypeList = new List<int>{1, 2};
    [ConfigValue] List<List<int>> listTypeList = new List<List<int>>{
        new List<int>{1, 2},
        new List<int>{3, 4}
    };
    [ConfigValue] List<Dictionary<string, string>> dictionaryTypeList = new List<Dictionary<string, string>>{
        new Dictionary<string, string>{ {"k1", "v1"}, {"k2", "v2"} },
        new Dictionary<string, string>{ {"k3", "v3"}, {"k4", "v4"} },
    };
    [ConfigValue] List<ExampleClass> nonGenericClassList = new List<ExampleClass>{
        new ExampleClass{ a = 1, b = 2 },
        new ExampleClass{ a = 3, b = 4 }
    };
}

class ExampleClass {
    [ConfigValue] int a;
    [ConfigValue] int b;
}
```
```
- simpleTypeList :
-- 1
-- 2

- listTypeList :
-- :
--- 1
--- 2
-- :
--- 3
--- 4

- dictionaryTypeList :
-- :
--- k1 = v1
--- k2 = v2
-- :
--- k3 = v3
--- k4 = v4

- nonGenericClassList :
-- :
--- a = 1
--- b = 2
-- :
--- a = 3
--- b = 4
```
An equivalent (whitespace minimized) notation would look like this.
```
-simpleTypeList:--1--2
-listTypeList:--:---1---2--:---3---4
-dictionaryTypeList:--:---k1=v1---k2=v2--:---k3=v3---k4=v4
-nonGenericClassList:--:---a=1---b=2--:---a=3---b=4
```

---

### Dictionary<TKey, TValue>

A `System.Collections.Generic.Dictionary<TKey, TValue>` dictionary.  
`TKey` may only be a Simple-Value Type.  
`TValue` can be any (de-)serializable type.

Example:

```c#
class ConfigClass {
    [ConfigValue] Dictionary<int, int> simpleTypeDict = new Dictionary<int, int>{
        {1, 2}, {3, 4}
    };
    [ConfigValue] Dictionary<bool, List<int>> listTypeDict = new Dictionary<bool, List<int>>{
        { false, new List<int>{1, 2} },
        { true, new List<int>{3, 4} }
    };
    [ConfigValue] Dictionary<Dictionary<string, string>> dictionaryTypeDict = new Dictionary<Dictionary<string, string>>{
        { "dict1", new Dictionary<string, string>{ {"k1", "v1"}, {"k2", "v2"} } },
        { "dict2", new Dictionary<string, string>{ {"k3", "v3"}, {"k4", "v4"} } }
    };
    [ConfigValue] Dictionary<ExampleEnum, ExampleClass> nonGenericClassDict = new Dictionary<ExampleEnum, ExampleClass>{
        { ExampleEnum.Value1, new ExampleClass{ a = 1, b = 2 } },
        { ExampleEnum.Value2, new ExampleClass{ a = 3, b = 4 } }
    };
}

enum ExampleEnum {
    Value1,
    Value2
}

class ExampleClass {
    [ConfigValue] int a;
    [ConfigValue] int b;
}
```
```
- simpleTypeDict  :
-- 1 = 2
-- 3 = 4

- listTypeDict :
-- false :
--- 1
--- 2
-- true :
--- 3
--- 4

- dictionaryTypeDict :
-- dict1 :
--- k1 = v1
--- k2 = v2
-- dict2 :
--- k3 = v3
--- k4 = v4

- nonGenericClassDict :
-- Value1 :
--- a = 1
--- b = 2
-- Value2 :
--- a = 3
--- b = 4
```

---

### NonGenericClasses

This can be any class without generic type arguments.  
(That means you can also deserialize directly to classes of the game!)  

If you are the owner of the class, you can add Attributes to configure
how your class will be (de)serialized. (see Attributes.md)

(De)serialization of NGC only respects Fields.  
That means you'll have to access properties through their corresponding field.  
For Auto-Properties you can simply use the Property-Name.

You can tell ConfigTools to deserialize NGC implicitly or explicitly.  
Implicit means that **all** fields of the class will be deserialized,
whereas explicit only uses fields with the `ConfigValue` attribute.

Example:
```c#
class ConfigClass {
    public int intField = 1;
    private int _intProperty = 2;
    
    int IntProperty {
        get => _intProperty;
        set => _intProperty = value;
    }
    
    int IntAutoProperty { get; set; } = 3
}
```
```
- intField = 1
- _intProperty = 2
- IntAutoProperty = 3
```