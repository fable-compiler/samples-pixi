module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Import.Browser
open Fable.Import.JS

let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0x000000

let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let onLoaded (loader:PIXI.loaders.Loader) (res:PIXI.loaders.Resource) =

    // our json
    let config : string = !!res?emitter?data

    // container to hold our particles 
    // note: you can use a ParticleContainer if you only use one texture 
    // the render will be way faster!
    let container = PIXI.Container()
    app.stage.addChild(container) |> ignore

    let texture = PIXI.Texture.fromImage("../img/particle.png")
    let renderer : PIXI.WebGLRenderer = !!app.renderer
    
    // create our emitter 
    let emitter = PIXI.particles.Emitter( container, !![|texture|], config )
    
    // place it at the center of the screen
    emitter.updateOwnerPos(renderer.width * 0.5, renderer.height * 0.5)

    // our render loop
    let tick delta = 
      emitter.update (delta * 0.01)
      
    app.ticker.add(tick) |> ignore
    
    // start emitting particles
    emitter.emit <- true

    // start our Pixi app
    app.start()    

// We start by loading the emitter json configuration File
// to get our particle animation parameters
// This json is built using pixi particles online editor 
// you can find the editor here: http://pixijs.github.io/pixi-particles-editor/
let loader = PIXI.loaders.Loader()
loader.add("emitter", "../img/emitter.json") |> ignore
loader.load(onLoaded) |> ignore
