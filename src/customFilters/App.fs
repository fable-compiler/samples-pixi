module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x1099bb
)
let app = PIXI.Application(800.,600.,options)
Browser.document.body.appendChild(app.view) |> ignore

// Create background image
let renderer : PIXI.WebGLRenderer = !!app.renderer
let background = PIXI.Sprite.fromImage("../img/bkg-grass.jpg")
background.width <- renderer.width
background.height <- renderer.height
app.stage.addChild(background) |> ignore

// Stop application wait for load to finish
app.stop()

let mutable filter= Unchecked.defaultof<PIXI.Filter<obj>> 

// Handle the load completed
let onLoaded (loader:PIXI.loaders.Loader) (res:PIXI.loaders.Resource) =

    // Create the new filter, arguments: (vertexShader, framentSource)
    let fragmentSource : string = !!res?shader?data 
    filter <- PIXI.Filter("", fragmentSource )

    // Add the filter
    background.filters <- !^[|filter|]

    // Resume application update
    app.start()

let loader = PIXI.loaders.Loader()
loader.add("shader", "../img/shader.frag") |> ignore
loader.load(onLoaded) |> ignore

// Animate the filter
app.ticker.add(fun delta ->
  let uniforms = filter.uniforms
  let currentUniform : float = !!uniforms?customUniform
  uniforms?customUniform <- currentUniform + 0.04 * delta
) |> ignore