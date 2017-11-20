module App

open Fable.Import
open State
open Renderer

let awaitAnimationFrame() =
  Async.FromContinuations(fun (cont,_,_) ->
    Browser.window.requestAnimationFrame(Browser.FrameRequestCallback cont) |> ignore)

let start() =
  let events = ResizeArray()
  let rec stateMachine (smodel: StateModel) (rmodel: RenderModel option): Async<unit> = async {
    let! _ = awaitAnimationFrame()
    let newState =
      match smodel with
      | Waiting _ -> updateWaiting events smodel
      | Dragging _ -> updateDragging events smodel
    events.Clear()
    let newRender = render newState rmodel events.Add
    return! stateMachine newState (Some newRender)
  }
  let state = initState()
  stateMachine state None

start() |> Async.StartImmediate