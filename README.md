# Perse

Perse is a small language, aimed at string processing and templating. It is currently a heavy WIP.

## Capabilities (so far)

### Data types
- 64-bit Integer
- Boolean
- String

```
var a = 2;
var b = true;
var c = "myString";
```
### Output
```
puts("Hello, world!");
```

### Input
```
var string = read();
```

### Operators
```
+, -, *, /, !
```

### Comparers
```
==, !=, <, >
```

### For loop
```
var arr = [1,2,3];
for (item in arr) {
  puts(item);
}
```

### For loop with index
```
var arr = ["one","two","three"];
for (item, index in arr) {
  puts(index);
  puts(item);
}
// output: "0 one 1 two 2 three"
```

### Implicit return (optional)
```
var plusTwo = fn(x) {
  x + 2;
}
```

### Higher order functions
```
var add = fn(a, b) { a + b; };`

var twice = fn(f, x) {
  return f(f(x));
};

var addTwo = fn(x) {
  return x + 2;
};

twice(addTwo, 2); // => 6
```
A Fibonacci sequence example:  
```
var fibonacci = fn(x) {  
  if (x == 0) {
    0
  } else {
    if (x == 1) {
      1
    } else {
      fibonacci(x - 1) + fibonacci(x - 2);
    }
  }
};
```

### Everything is an expression
For example, you can use an if statement as a conditional statement:
```
var value = if (x > y) { x } else { y };
```

### Built-in library functions
```
var arr = [1,2,3];
var arrTwo = [4,5,6];
var testStr = "hey";

str.length(testStr);            // 3
str.concat(testStr, " there!"); // "hey there!"
list.first(arr);                // 1
list.length(arr);               // 3
list.reverse(arr);              // [3,2,1]
list.concat(arr, arrTwo);       // [1,2,3,4,5,6]
list.push(arr, 9);              // [1,2,3,9]
list.part(arr, fn(x) {x > 2});  // [[3], [1,2]]
int.parse("12");                // 12

```

## Roadmap
- [x] stdout
- [x] stdin
- [x] Arrays
- [x] Loops
- [x] Built-in functions
- [x] Strings
- [ ] Hashes
- [ ] Interpolated strings
- [ ] Templating
- [ ] A standard library
