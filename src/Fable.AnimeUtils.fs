module Fable.AnimeUtils

open Fable.Import.Animejs
open Fable.Core.JsInterop

let XY (target:'t) xFactor yFactor duration elasticity =
  jsOptions<anime.AnimeAnimParams> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item("x") <- xFactor
    o.Item("y") <- yFactor
  )         
