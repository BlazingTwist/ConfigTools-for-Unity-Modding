## (De)Serialization Attributes

These allow you to generate documentation for your config files
and change their visual structure.

---

### `ConfigValue`

The ConfigValue attribute allows you to specify these arguments:
- `emptyLinesAbove` : amount of empty lines to place above this field
- `indentation` : amount of indentation (tabs) this field should gain
- `comments` : single-line comments to document this field
- `name` : the name that is used when serializing this field
- `keyFormatOption` : specify a different key formatting behaviour for this field
- `valueFormatOption` : specify a different value formatting behaviour for this field

FormatOptions:
- `UseDefault` - uses the provided FormatOption or `QuoteValue` if none is provided.
- `AlwaysQuote` - all values will be formatted as `"some va-lue"`
- `Escape` - special characters will be escaped, all values are unquoted: `some\ va\-lue`
- `QuoteCharacter` - special characters will be quoted, otherwise unquoted: `some" "va"-"lue`
- `QuoteValue` - quote value if it contains special character, otherwise unquoted: `"some va-lue"`

Example:
```c#
class ConfigClass {
    class ConfigSubClass {
        [ConfigValue(name: "first", indentation: -1, comments: "Negative indentation")]
        int intValue1 = 1;
        [ConfigValue(name: "second", indentation: 1, comments: "Positive indentation")]
        int intValue2 = 2;
    }
    
    [ConfigValue]
    int intValue = 0;
    
    [ConfigValue(indentation: 1, emptyLinesAbove: 1, comments: new[] {"subConfig", "example"})]
    ConfigSubClass subConfig = new ConfigSubClass();
}
```
```c#
- intValue = 0

    // subConfig
    // example
    - subConfig :
// Negative indentation
-- first = 1
        // Positive indentation
        -- second = 2
```

---

### `ConfigComment`

The ConfigComment attribute allows you to add more structure / documentation to
your configs with these arguments:
- `emptyLinesAbove` : amount of empty lines to place above this comment
- `indentation` : amount of indentation (tabs) this comment should gain
- `comments` : multi-line comments

(a slightly more practical) Example:
```c#
class ConfigClass {
    [ConfigComment(comments: "Vertical Camera Controls")]
    [ConfigValue(indentation: 1)]
    KeyBind cameraUp = new KeyBind(new List<Key> { new Key(KeyCode.W, false), new Key(KeyCode.LeftControl, true) });

    [ConfigValue(indentation: 1, emptyLinesAbove: 1)]
    KeyBind cameraDown = new KeyBind(new List<Key> { new Key(KeyCode.S, false), new Key(KeyCode.LeftControl, true) });

    [ConfigComment(emptyLinesAbove: 1, comments: "Horizontal Camera Controls")]
    [ConfigValue(indentation: 1)]
    KeyBind cameraLeft = new KeyBind(new List<Key> { new Key(KeyCode.A, false), new Key(KeyCode.LeftControl, true) });

    [ConfigValue(indentation: 1, emptyLinesAbove: 1)]
    KeyBind cameraRight = new KeyBind(new List<Key> { new Key(KeyCode.D, false), new Key(KeyCode.LeftControl, true) });
}
```
```
/* Vertical Camera Controls */
    - cameraUp :
    -- W
    -- !LeftControl

    - cameraDown :
    -- S
    -- !LeftControl

/* Horizontal Camera Controls */
    - cameraLeft :
    -- A
    -- !LeftControl

    - cameraRight :
    -- D
    -- !LeftControl
```

---

### `SingleFieldType`

The SingleFieldType-Attribute can be used if your class consists of only one Field.
(The goal here is to make the resulting config file more concise)

For Example, **without SingleFieldType**:
```c#
class ConfigClass {
    class SubClass {
        int someInt = 1;
    }
    SubClass subClass1 = new SubClass();
    SubClass subClass2 = new SubClass();
}
```
```
- subClass1 :
-- someInt = 1
- subClass2 :
-- someInt = 1
```
**with SingleFieldType**:
```c#
class ConfigClass {
    [SingleFieldType(fieldName: nameof(someInt))]
    class SubClass {
        [ConfigValue] int someInt = 1;
    }
    [ConfigValue] SubClass subClass1 = new SubClass();
    [ConfigValue] SubClass subClass2 = new SubClass();
}
```
```
- subClass1 = 1
- subClass2 = 1
```

For a more practical Example, you can take a look at
`BlazingTwistConfigTools.configurables.KeyBind`