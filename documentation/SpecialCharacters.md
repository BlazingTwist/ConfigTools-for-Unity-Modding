## Special characters:

### Object-Depth indicator `-`

used to express the parent of a value, for example C#

```c#
class ConfigClass {
    [ConfigValue] List<string> stringList;
}
```

would be expressed as

```
- stringList :
-- firstString
-- "second String"
```

here, both `firstString` and `second String` belong to the stringList.

---

### Assignment Operator `=` and `:`

both of these operators are interpreted identically, but recommended form is to
assign leaf nodes with `=` and branch nodes with `:`

for example:

```c#
class ConfigClass {
    [ConfigValue] string someString;
    [ConfigValue] Dictionary<int, string> someDictionary;
}
```

would be expressed as

```
- someString = "firstString"
- someDictionary :
-- 2 = "secondString"
-- 3 = "thirdString"
```

---

### Whitespace

Normally, all whitespace is ignored. If you wish to keep whitespace,
you can put it in a quoted string, or use the escape character to "escape" it.  
All linebreaks (`\n`, `\r\n` and `\r`) will be normalized to `\n` if they are escaped.

For a reference on which kind of characters are consider whitespace,
you can refer to the MS-Docs `Char.IsWhiteSpace`

---

### Null `~`

Since whitespace is ignored, you can declare "no value" (null) with `~`.  
Types that are not nullable (e.g. primitives) will be initialized to their [default value](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/default-values).  
Naturally, you can escape the null character by preceding it with an escape character. `\~`

Notably, the only scenario in which the null character is strictly required is in list-values.  
For example, assuming this config class:
```c#
class ConfigClass {
    [ConfigValue] string stringValue;
    [ConfigValue] List<string> stringList;
}
```
`- stringValue = ~` and `- stringValue =` are equal, but  
`- stringList : -- ~ -- ~` and `- stringList : -- --` are not.  
This is, because the first version describes two null-values at object depth `2`,  
whereas the second version describes one null-value at object depth `4`

---

### Quoted Strings `"`

Quoted strings preserve all contained characters.  
The only special character in quoted strings is `"` which opens/closes the string.  
You can escape `"` with the escape character, both inside and outside of quoted strings.

---

### Comments `//` and `/* */`

`//` Declares a single-line comment,
all characters to the right on the same line will be ignored.  
`/*` Opens a ranged comment (multi-line), all characters (including line-breaks)
will be ignored.  
`*/` Closes the ranged comment.

You can use the escape character to escape `//`, `/*` and `*/`

---

### Escape Character ` \ `

You can escape any character by prefixing it with `\ `  
For example, the unquoted string `\abc\ \-\:\=\" \//./\/.\/*./\*.\*/.*\/`  
Will be read as this literal `abc -:="//.//./*./*.*/.*/`

You can escape single-line comments with `\//`, `/\/` or `\/\/`  
all of these will be read as the literal `//`  
The same works for ranged comments.

You can also escape line-breaks, by placing `\ ` at the end of the line.  
This will be parsed as the literal `\n`