module Fable.Pixi

open Fable.Core
open Fable.Import.Pixi
open Fable.Core.JsInterop

type ExtendedSprite<'T> (texture:PIXI.Texture,data: 'T) =
  inherit PIXI.Sprite(texture)
  member this.Data = data

[<RequireQualifiedAccess>]
module Event = 

  type EventHandler = PIXI.interaction.InteractionEvent->unit

  [<StringEnum>]
  type PixiEvent =
    | Pointerdown
    | Pointerup  
    | Pointeroutside
    | Pointermove  
    | Pointertap

  let attach (ev: PixiEvent) (handler: EventHandler) (sprite: PIXI.Sprite) =
    sprite.on(!!(string ev), handler) |> ignore
    sprite

[<RequireQualifiedAccess>]
module Assets = 
  let mutable textures = Map.empty<string,PIXI.Texture> 
  let mutable objFiles = Map.empty<string,obj> 
 
  let addTexture name texture = 
    textures <- textures.Add(name,texture) 

  let addObj name text = 
    objFiles <- objFiles.Add(name,text)     

  let getTexture name = 
     textures.TryFind name

  let getObj name = 
     objFiles.TryFind name     

[<RequireQualifiedAccess>]
module Layers =      
  let mutable layers = Map.empty<string,PIXI.Container> 

  let add name (root:PIXI.Container) =
    let c = PIXI.Container() 
    layers <- layers.Add(name,c)
    root.addChild c     

  let get name = 
     layers.TryFind name     

  let remove name = 
     match (layers.TryFind name) with 
     | Some layer -> 

        // remove children if found any
        try 
          layer.children
            |> Seq.iteri( fun i child -> 
              layer.removeChild( layer.children.[i] ) |> ignore
            ) 
         with e -> printfn "No children where found in layer %s" name        

        // remove layer from parent
        layer.parent.removeChild layer |> ignore

        layers <- layers.Remove name
     | None -> failwith (sprintf "unknwon layer %s" name)

[<RequireQualifiedAccess>]
module SpriteUtils =

  let fromTexture name =      
    let texture = Assets.getTexture name
    match texture with
    | Some t -> 
      PIXI.Sprite t
    | None ->  failwith (sprintf "unknown texture %s" name) 

  let addToLayer name sprite =
    let container = Layers.get name
    match container with
    | Some c -> 
      c.addChild sprite
    | None ->  failwith (sprintf "unknown layer %s" name) 

  let scaleTo x y (sprite:PIXI.Sprite) = 
    let scale : PIXI.Point = !!sprite.scale
    scale.x <- x
    scale.y <- y
    sprite

  let moveTo x y (sprite:PIXI.Sprite) = 
    let position : PIXI.Point = !!sprite.position
    position.x <- x
    position.y <- y
    sprite

  let setAnchor x y (sprite:PIXI.Sprite) = 
    let anchor : PIXI.Point = !!sprite.anchor
    anchor.x <- x
    anchor.y <- y
    sprite

  let setAlpha a (sprite:PIXI.Sprite) = 
    sprite.alpha <- a 
    sprite
