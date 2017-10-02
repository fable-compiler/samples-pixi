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

[<Literal>]
let duration = 4000.

[<Literal>]
let easing= "linear"


[<Literal>]
let dragonPath  ="M323.587,152.743c-11.562,-0.222 -20.376,-1.334 -19.67,-3.324c3.879,-10.889 -2.155,-21.52 0.546,-21.175c4.907,0.577 9.28,1.016 13.291,1.333c-13.203,-9.955 -14.12,-18.019 -15.877,-36.097c0,0 -8.476,-11.493 -37.436,5.2c16.261,-21.893 31.431,-16.693 34.362,-15.543c0.201,-2.471 -4.051,-3.994 -13.073,-11.694c15.343,7.211 24.45,6.321 24.45,6.321c-0.111,-0.556 0.369,-6.801 -3.854,-9.372c-0.295,-0.161 -0.455,-0.253 -0.455,-0.253c0.157,0.08 0.309,0.164 0.455,0.253c2.607,1.427 15.749,8.292 22.098,5.349c9.396,-4.338 30.944,10.315 42.781,5.373c2.327,-0.977 -1.236,4.367 0.546,7.901c-1.954,5.488 -37.006,18.848 -48.67,15.371c-3.592,25.829 30.023,20.083 44.906,25.542l-0.004,0.001c5.509,2.189 5.755,19.644 -1.691,21.519c-5.894,1.478 -14.858,2.462 -24.168,2.958c0.279,3.232 -3.452,17.625 -28.094,60.997c0,0 10.445,-28.57 9.557,-60.66Z"

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


    let dragon = PIXI.Sprite.fromImage("../img/dragon.png")
    dragon.anchor.set 0.5 |> ignore
    dragon.position?x <- x - 30.
    dragon.position?y <- y - 10.
    app.stage.addChild dragon |> ignore

    let style = jsOptions<PIXI.TextStyle>( fun o -> 
        o.fontFamily<- !^"Josefin Sans"
        o.fontSize<- !^36.
        o.fill<- !!"#1292FF"
    )

    let richText = PIXI.Text("Fable",style)
    richText.x <- x
    richText.y <- y + 120.
    richText.anchor.set 0.5 |> ignore

    app.stage.addChild richText |> ignore

    // create our emitter 
    let emitter = PIXI.particles.Emitter( emitterCOntainer, !![|texture|], config )
    emitter.updateOwnerPos(x,y)

    // Make the timeline loop until the end of the woooooooorld!
    let timelineOptions = 
      jsOptions<anime.AnimeTimelineInstance>( fun o -> 
        o.loop <- !!true        
      )
    
    // create our tweening timeline
    let timeline = anime.Globals.timeline(!!timelineOptions)

    let path = anime.Globals.path(!!"#motionPath path")
    
    let options = 
      jsOptions<anime.AnimeAnimParams> (fun o ->
          o.easing <- !!easing
          o.duration <- !!duration
          o.targets <- !!emitter.ownerPos
          o.Item("x") <- !!path("x")
          o.Item("y") <- !!path("y")
        )

    // prepare our animation
    timeline.add options |> ignore 

    // our render loop
    let tick delta = 
      emitter.update (delta * 0.01)
      
    app.ticker.add(tick) |> ignore
    
    // start emitting particles
    emitter.emit <- true

    // start our Pixi app
//    app.start()    

    let start delta = app.start()
    Browser.window.setTimeout( Func<_,_>start , 500. ) |> ignore

// We start by loading the emitter json configuration File
// to get our particle animation parameters
// This json is built using pixi particles online editor 
// you can find the editor here: http://pixijs.github.io/pixi-particles-editor/
let loader = PIXI.loaders.Loader()
loader.add("emitter", "../img/dragon.json") |> ignore
loader.load(onLoaded) |> ignore
