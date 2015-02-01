
// Quick little file upload server

open System
open System.IO
open System.Net

// Serve from arg or from cwd
let args = fsi.CommandLineArgs
let cwd = if args.Length > 1 then Path.GetFullPath args.[1] else Environment.CurrentDirectory

let server = new HttpListener ()
server.Prefixes.Add ("http://*:1234/")
server.Start ()
printfn "Serving %s at %O" cwd (Seq.head server.Prefixes)

while true do
    try
        let c = server.GetContext ()
        let partialPath = c.Request.Url.AbsolutePath.Substring 1
        let path = Path.Combine (cwd, partialPath)
        match c.Request.HttpMethod with
        | "POST" ->
            use f = File.OpenWrite path
            c.Request.InputStream.CopyTo (f)
            f.Flush ()
            printfn "POSTED %s" path
            c.Response.StatusCode <- 200
            c.Response.Close ()
        | "GET" ->
            c.Response.StatusCode <- 200
            let w = new StreamWriter (c.Response.OutputStream)
            w.WriteLine ("Hello from fserve: {0}", path)
            w.Flush ()
            c.Response.Close ()
        | x ->
            c.Response.StatusCode <- 405
            c.Response.Close ()
    with ex ->
        printfn "WHOOPS %O" ex
