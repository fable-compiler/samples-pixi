module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

// create a new Sprite from an image path
let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0x000000

let app = PIXI.Application(400., 400., options)

Browser.document.body.appendChild(app.view) |> ignore

let onAssetsLoaded() = 

  let frames = 
    [|
      for i in 0..29 do
        let index = if i < 10 then "00" else sprintf "%i" i
        let name = sprintf "rollSequence00%s.png" index
        let texture = PIXI.Texture.fromFrame name
        yield 
          texture
    |]

  let anim = PIXI.extras.AnimatedSprite !!frames
  let renderer : PIXI.WebGLRenderer = !!app.renderer
  anim.x <- renderer.width / 2.
  anim.y <- renderer.height / 2.
  anim.anchor.set(0.5)
  anim.animationSpeed <- 0.5
  anim.play()

  app.stage.addChild anim |> ignore
  app.ticker.add( fun delta -> anim.rotation <- anim.rotation + 0.01 ) |> ignore

let loader = PIXI.loaders.Loader()
loader.add("../img/fighter.json") |> ignore
loader.load(onAssetsLoaded) |> ignore
