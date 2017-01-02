# Perse

Perse is a small language, aimed at string processing and templating. It is currently a heavy WIP.

## Capabilities (so far)

### Data types
- 64-bit Integer
- Boolean

### Higher order functions
```
var add = fn(a, b) { a + b; };`

let twice = fn(f, x) {
  return f(f(x));
};

let addTwo = fn(x) {
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
- [ ] stdin/stdout
- [ ] Arrays
- [ ] Loops
- [ ] Strings
- [ ] Hashes
- [ ] Interpolated strings
- [ ] Hashes
- [ ] Templating
- [ ] A standart library
