# Commit attribution

Every commit made with Claude Code's help must end its commit message with:

```
Co-Authored-By: Claude <model name> <noreply@anthropic.com>
```

(e.g. `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`, matching whichever
model made the commit.) This applies to every commit, not just large ones - check
before running `git commit` that the trailer is actually in the message, since it's
easy to drop on a quick follow-up commit.
