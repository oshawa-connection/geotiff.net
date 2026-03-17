# Remote testing

## Servers

Different servers respond in different ways to range requests; we need to support them all.

Some servers might just respond with the whole file if they receive a range request.
`python -m http.server` does this.