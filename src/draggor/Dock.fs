module Dock

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Pixi
open Fable.Import.JS
open Fable.Pixi
open Types

let prepare container stage renderer= 
  
  // create our cogs for our dock 
  let rec makeCogs cogs totalWidth (list:ExtendedSprite<CogData> list)  (container:PIXI.Container) (renderer: PIXI.WebGLRenderer )= 
    let dockY = renderer.height * 0.9
    match cogs with
    | [] -> // center our cogs on screen
      let margin = (renderer.width - totalWidth) * 0.5
      list
        |> List.map( fun cog -> 
          let (x,y) = cog.Data.StartPosition 
          (Cog.moveTo (x+margin) y >> Cog.backTo) cog
        )

    | head :: remains -> // add next cog to the dock
      let margin = 30.
      let width = (Cog.scaleFactor head) * Cog.cogWidth + margin

      let cog = 
        Cog.make head 
          |> Cog.moveTo totalWidth dockY
          |> Cog.scaleTo
            
      makeCogs remains (totalWidth+width) (list @ [cog]) container renderer    
  
  makeCogs 
    [Tiny;Small;Medium;Large] 0. [] container renderer
