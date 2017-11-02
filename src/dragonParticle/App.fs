module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Import.Animejs
open Fable.Import.Browser
open Fable.Import.JS
open Fable.AnimeUtils

[<Literal>]
let duration = 4000.

[<Literal>]
let easing= "linear"

let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0x000000

let app = PIXI.Application(700., 300., options)
Browser.document.body.appendChild(app.view) |> ignore

let onLoaded (loader:PIXI.loaders.Loader) (res:PIXI.loaders.Resource) =

    // our json
    let config : string = !!res?emitter?data

    // container to hold our particles
    // note: you can use a ParticleContainer if you only use one texture
    // the render will be way faster!

    let texture = PIXI.Texture.fromImage("../img/particle.png")
    let renderer : PIXI.WebGLRenderer = !!app.renderer

    let x = renderer.width * 0.5
    let y = renderer.height * 0.5


    let emitterCOntainer = PIXI.Container()
    app.stage.addChild(emitterCOntainer) |> ignore


    // put dragon ABOVE our particle emitter
    // to get the particles shining below
    let dragon = PIXI.Sprite.fromImage("../img/dragon.png")
    dragon.anchor.set 0.5 |> ignore
    dragon.position?x <- x - 30.
    dragon.position?y <- y - 10.
    dragon.alpha <- 0.3
    app.stage.addChild dragon |> ignore

    // create our emitter
    let emitter = PIXI.particles.Emitter( emitterCOntainer, !![|texture|], config )
    emitter.updateOwnerPos(x,y)

    // Make the timeline loop until the end of the woooooooorld!
    let timelineOptions =
      jsOptions<AnimInput>( fun o ->
        o.loop <- !!true
      )

    // create our tweening timeline
    let timeline = GetTimeline (Some timelineOptions)

    // get the path values from the div in index.html
    let path = anime.path "#motionPath path"

    let options =
      jsOptions<AnimInput> (fun o ->
          o.easing <- !!easing
          o.duration <- !!duration
          o.targets <- !!emitter.ownerPos
          o.Item("x") <- !!path "x"
          o.Item("y") <- !!path "y"
        )

    // prepare our animation
    timeline.add options |> ignore

    // our render loop
    let tick delta =
      emitter.update (delta * 0.01)

    app.ticker.add(tick) |> ignore

    // start emitting particles
    emitter.emit <- true

    app.start()

// We start by loading the emitter json configuration File
// to get our particle animation parameters
// This json is built using pixi particles online editor
// you can find the editor here: http://pixijs.github.io/pixi-particles-editor/
let loader = PIXI.loaders.Loader()
loader.add("emitter", "../img/dragon.json") |> ignore
loader.load(onLoaded) |> ignore
