# MidcoUsageChecker

Unlike certain very unpopular ISPs, Midco doesn’t have artificial data caps on their residential customers.

Being the sort that likes to monitor my usage, this .NET Standard 2.0 library provides a client which logs into the Midco website, parses out the JSON data representing the daily and monthly usage, then returns it to the caller as a set of POCOs.

Included .NET 6 example app demonstrates the basic usage, just specify your username & password in the static variables.

WARNING: This is an unofficial client, unsupported by Midco and myself, so if things stop working or an account is blocked, responsibility and liability is on the user of this code, not the author.

Note that the session cookie is good for ~10 minutes, so getting updates (if desired) that often will usually keep things alive, however at some point I may do better at detecting loss of the authenticated session and support automatic reconnection. For my purposes, this isn’t all too important right now.
