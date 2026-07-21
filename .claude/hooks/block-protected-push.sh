#!/bin/bash
# Blocks pushes to protected branches (main, master, develop).
# This hook is invoked by Claude Code PreToolUse / Devin CLI hooks.

PROTECTED_BRANCHES="^(main|master|develop)$"

# Inspect the current git push command, if any.
PUSH_CMD=""
if [ -n "$BASH_COMMAND" ]; then
    PUSH_CMD="$BASH_COMMAND"
else
    # Try to detect a git push in the process tree.
    PUSH_CMD=$(ps -eo args= 2>/dev/null | grep -E '^git\s+push' | head -n 1 || true)
fi

# Also check arguments passed to the hook (some tools pass the tool call as args).
for arg in "$@"; do
    if echo "$arg" | grep -Eq "$PROTECTED_BRANCHES"; then
        echo "❌ Push to protected branch '$arg' is not allowed." >&2
        exit 1
    fi
done

if [ -n "$PUSH_CMD" ]; then
    # Extract ref arguments from "git push [options] remote ref[:ref] ..."
    for ref in $(echo "$PUSH_CMD" | tr ' ' '\n' | grep -E '^[^-]' | tail -n +2); do
        branch=$(echo "$ref" | sed -E 's/.*://; s/.*\///')
        if echo "$branch" | grep -Eq "$PROTECTED_BRANCHES"; then
            echo "❌ Push to protected branch '$branch' is not allowed." >&2
            exit 1
        fi
    done
fi

exit 0
