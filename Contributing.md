# Contributing

I assume you're reading this doc because you want to help improve NGraphics. That's great! Thank you

But before you get started, please note the following:


## Whitespace Changes will be Rejected

The [official code formatting](http://www.mono-project.com/community/contributing/coding-guidelines/) follows mono.

Basically, it goes against the default Visual Studio style and is darned close to MonoDevelop's defaults.

If you submit a pull request with formatting changes in parts of code not otherwise edited, the PR will be rejected.

For tabs, I prefer actual tab characters. You will notice the codebase bounces back and forth
between the two since I keep getting caught up in other people's holy wars.

Same goes for line endings - some are Windows style while others are correct.

If either of these bothers you, upgrade your text editor.


## Adding Dependencies

Don't.


## Unit Tests

If you add a significant feature, there needs to be some test coverage of that code. I don't care about 100% coverage,
but I need proof your code works.
