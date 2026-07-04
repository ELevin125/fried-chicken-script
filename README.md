<div align="center">

# Fried Chicken Script

<img src="assets/logo.png" width="620">

</div>

Fried Chicken Script is a KFC-themed scripting language written in C#. It's an interpreted scripting language inspired by the simplicity of Python and the flexibility of JavaScript.

## Usage

If a script file is supplied, the interpreter executes it.

```fc
orderUp("Hello, World!")
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

## Keywords

| Keyword | Description |
|----------|-------------|
| `ingredient` | Declare a variable |
| `recipe` | Declare a function |
| `withExtra` | Declare recipe parameters |
| `bucket` | Declare an object |
| `serve` | Return a value from a recipe |
| `orderUp()` | Print to the console |
| `fryWhile` | While loop |
| `COOKED` | Boolean `true` |
| `RAW` | Boolean `false` |

---

## Examples

The `examples/` directory contains sample programs if you don't know where to start.