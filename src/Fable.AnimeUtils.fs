module Fable.AnimeUtils

open Fable.Import.Animejs
open Fable.Core.JsInterop

let XY (target:'t) xFactor yFactor duration elasticity =
  jsOptions<AnimInput> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item("x") <- xFactor
    o.Item("y") <- yFactor
  )

let SingleParameter (target:'t) parameterName paramaterFactor duration elasticity =
  jsOptions<AnimInput> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item(parameterName) <- paramaterFactor
  )

let GetPath (elementId:string) =
  let instance = anime
  instance.path !!elementId

let GetInstance (options: AnimInput option) =
  let instance = anime
  match options with 
  | Some o -> 
    instance.Invoke o
  |
   None -> 
    instance.Invoke()

let GetTimeline (options: AnimInput option) =
  let instance = anime
  match options with 
  | Some o -> 
    instance.timeline o
  | None -> 
    instance.timeline()
