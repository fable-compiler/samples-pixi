module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi

type Model =
  { app: PIXI.Application
    renderer: PIXI.WebGLRenderer
    bunnies: Map<Guid, PIXI.Sprite> }

type Msg =
  | AddBunny of Guid
  | RemoveBunny of Guid

let update msg (model: Model) =
  match msg with
  | AddBunny id ->
    // create a new Sprite from an image path
    let bunny = PIXI.Sprite.fromImage("../img/fable_logo_small.png")
    // center the sprite's anchor point
    bunny.anchor.set(0.5)
    bunny.x <- model.renderer.width * 0.5
    bunny.y <- model.renderer.height * 0.5
    model.app.stage.addChild(bunny) |> ignore
    { model with bunnies = Map.add id bunny model.bunnies }, []
  | RemoveBunny id ->
    match Map.tryFind id model.bunnies with
    | Some bunny ->
      model.app.stage.removeChild(bunny) |> ignore
      { model with bunnies = Map.remove id model.bunnies }, []
    | None ->
      model, []

let init() =
  let addBunny dispatch =
    Guid.NewGuid() |> AddBunny |> dispatch
  let app = PIXI.Application(400., 400., jsOptions(fun o ->
    o.backgroundColor <- Some 0x000000))
  Browser.document.body.appendChild(app.view) |> ignore
  { app = app
    renderer =
      match app.renderer with
      | U2.Case1 x -> x
      | U2.Case2 _ -> failwith "Unexpected CanvasRenderer"
    bunnies = Map.empty }, [addBunny]

open Elmish
open Elmish.Animation
open Elmish.Debug
open Elmish.HMR

let tick delta (model: Program.HMRModel<Model>) =
  for KeyValue(_,bunny) in model.UserModel.bunnies do
    // just for fun, let's rotate mr rabbit a little
    // delta is 1 if running at 100% performance
    // creates frame-independent tranformation
    bunny.rotation <- bunny.rotation + 0.001 * delta
  model

let view (model: Model) _ =
  model.renderer.render(model.app.stage)

let subscribeToAnimationPause (toggle: unit->unit) =
    Browser.window.addEventListener_keyup(fun ev ->
        if ev.keyCode = 80. then // [P]ause
            toggle()
        null)

Program.mkProgram init update view
|> Program.withHMR
|> Program.withAnimation tick subscribeToAnimationPause
// |> Program.withDebuggerDebounce 2000
|> Program.run

// Listen for animate update
// app.ticker.add(tick) |> ignore
