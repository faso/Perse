# Perse

Perse is a small language, aimed at string processing and templating. It is currently a heavy WIP.

## Capabilities (so far)

### Data types
- 64-bit Integer
- Boolean

```
var a = 2;
var b = true;
```

### Operators
```
+, -, *, /, ==, !=, !
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

## Roadmap
- [ ] stdout
- [ ] stdin
- [ ] Arrays
- [ ] Loops
- [ ] Strings
- [ ] Hashes
- [ ] Interpolated strings
- [ ] Templating
- [ ] A standart library
