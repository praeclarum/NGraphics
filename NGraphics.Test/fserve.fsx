
// Quick little file upload server

open System
open System.Net

let cwd = Environment.CurrentDirectory

let server = new HttpListener ()
server.Prefixes.Add ("http://*:1234/")
server.Start ()
printfn "Running at %O" (Seq.head server.Prefixes)

while true do
    try
        let c = server.GetContext ()
        let partialPath = c.Request.Url.AbsolutePath.Substring 1
        let path = System.IO.Path.Combine (cwd, partialPath)
        match c.Request.HttpMethod with
        | "POST" ->
            let partialPath = c.Request.Url.AbsolutePath.Substring 1
            let path = System.IO.Path.Combine (cwd, partialPath)
            use f = System.IO.File.OpenWrite path
            c.Request.InputStream.CopyTo (f)
            f.Flush ()
            printfn "POSTED %s" path
            c.Response.StatusCode <- 200
            c.Response.Close ()
        | "GET" ->
            c.Response.StatusCode <- 200
            let w = new System.IO.StreamWriter (c.Response.OutputStream)
            w.WriteLine ("Hello from fserve: {0}", path)
            w.Flush ()
            c.Response.Close ()
        | x -> failwithf "Huh? %s" x
    with ex ->
        printfn "WHOOPS %O" ex
