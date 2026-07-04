<div align="center">

# Fried Chicken Script

<img src="assets/logo.png" width="620">

</div>

Fried Chicken Script is a KFC-themed scripting language written in C#. It's an interpreted scripting language with a writing style similar to JavaScript

## Usage

```fc
orderUp("Hello, Chicken!")
```

### Running

Run a script directly:

```bash
friedchicken program.fc
```

Or start the interactive shell (REPL):

```bash
friedchicken
```

---

## Variables

Variables are declared using `ingredient`.

```fc
ingredient chef = "Colonel"
ingredient chickenCount = 8
ingredient kitchenOpen = COOKED
```

---

## Recipes (Functions)

Functions are called **recipes**, declared with the `recipe` keyword.

```fc
recipe greetCustomer withExtra: name {
    orderUp("Welcome, " + name + "!")
}

greetCustomer("Colonel")
```

Return a value using `serve`.

```fc
recipe totalPieces withExtra: buckets, piecesPerBucket {
    serve buckets * piecesPerBucket
}
```

### Parameters

Use **`withExtra`** to declare parameters for a recipe.

```fc
recipe fryChicken withExtra: pieces, seasoning {
    orderUp("Frying " + pieces + " pieces with " + seasoning)
}
```

If a recipe doesn't need any parameters, simply leave `withExtra` out.

```fc
recipe ringBell {
    orderUp("Order ready!")
}
```

---

## Buckets (Objects)

Objects are declared using `bucket`.

```fc
bucket FamilyMeal {
    ingredient pieces = 8

    recipe remainingPieces {
        serve pieces
    }
}
```

---

## Loops

```fc
ingredient batch = 0

fryWhile (batch < 5) {
    orderUp("Cooking batch " + batch)
    batch = batch + 1
}
```

Stop a loop early with `closeShop` (like `break`).

```fc
ingredient batch = 0

fryWhile (COOKED) {
    if (batch >= 5) {
        closeShop
    }
    orderUp("Cooking batch " + batch)
    batch = batch + 1
}
```

---

## Conditionals

Branch with `if`, `else if`, and `else`.

```fc
ingredient pieces = 12

if (pieces >= 12) {
    orderUp("Family bucket!")
}
else if (pieces >= 6) {
    orderUp("Sharing box")
}
else {
    orderUp("Snack box")
}
```

Constants: `COOKED` (true), `RAW` (false), and `EMPTY` (null).

---

## Lists

Lists can store multiple values.

```fc
ingredient menu = [
    "Original",
    "Zinger",
    "Hot Wings"
]

orderUp(menu[0])
```

Lists support adding, removing, and checking their length.

```fc
menu.add("Popcorn Chicken")
menu.remove("Original")

orderUp(menu.length)
```


---

## Builtins

Builtins are called like any recipe. They're reserved names, so you can't redefine them.

| Builtin | Description |
|----------|-------------|
| `orderUp(value)` | Print to the console |
| `takeOrder()` | Read a line of input |
| `random()` / `random(n)` | Random `[0,1)` double, or whole number in `[0, n)` |
| `randomSeed(n)` | Seed the random generator for reproducible runs |
| `toNumber(text)` | Parse text to a number, or `EMPTY` if it isn't one |
| `min(a, b, ...)` / `max(a, b, ...)` | Smallest / largest of its arguments |
| `abs(x)` | Absolute value |
| `round(x)` / `round(x, n)` | Round to a whole number, or to `n` decimals |
| `letItCook(ms)` | Pause for `ms` milliseconds |
| `wipeCounter()` | Clear the console |

---

## Keywords

| Keyword | Description |
|----------|-------------|
| `ingredient` | Declare a variable |
| `recipe` | Declare a function |
| `withExtra` | Declare recipe parameters |
| `bucket` | Declare an object |
| `serve` | Return a value from a recipe |
| `fryWhile` | While loop |
| `if` / `else if` / `else` | Conditionals |
| `closeShop` | Break out of a loop |
| `COOKED` | Boolean `true` |
| `RAW` | Boolean `false` |
| `EMPTY` | Null / no value |

---
## Example Game
### Fried Chicken Tycoon

`examples/restaurantSimulator.fc` is a full playable game built entirely in Fried Chicken Script — a turn-based shop simulator where you buy stock, set prices, and keep customers happy to hit a gold target before the week is out.

```bash
friedchicken examples/restaurantSimulator.fc
```

It exercises nearly the whole language at once: buckets with recipes and the `myBucket` receiver, lists, `fryWhile` loops with `closeShop`, `if` / `else if` / `else`, and the builtins `random`, `toNumber`, `min` / `max`, `round`, `letItCook`, and `wipeCounter` (for validated input, a reputation system, tips, random daily events, an animated lunch rush, and a repainting screen).

### Other Examples

The `examples/` directory contains sample programs if you don't know where to start.

---
## VS Code Syntax Highlighting

A Visual Studio Code syntax highlighting extension is included in the `VScodeSyntaxHighlighting/` directory.

### Option 1 - Install from source

Copy the `VScodeSyntaxHighlighting` folder into your VS Code extensions directory:

| Platform | Location |
|----------|----------|
| Windows | `%USERPROFILE%\.vscode\extensions\` |
| Linux | `~/.vscode/extensions/` |
| macOS | `~/.vscode/extensions/` |

Restart VS Code and `.fc` files will automatically use Fried Chicken Script syntax highlighting.

### Option 2 - Package as a VSIX

If you have the VS Code Extension Manager (`vsce`) installed:

```bash
cd VScodeSyntaxHighlighting
vsce package
code --install-extension *.vsix
```


