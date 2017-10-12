module Types 

open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Import.Pixi.Sound
open Fable.Import.Animejs
open Fable.Pixi
open Fable.Core.JsInterop

type Size = 
  | Tiny 
  | Small
  | Medium 
  | Large

type CogData = {
  Size: Size
  mutable Target: float*float
  mutable StartPosition: float*float
  mutable IsDragging: bool
  mutable Interaction : PIXI.interaction.InteractionData option    
  mutable IsFound: bool
}

type PointerId = int
type Msg =
  | OnMove of ExtendedSprite<CogData> * PointerId

type Layer = PIXI.Container


[<RequireQualifiedAccess>]
type CogState = 
  | Init
  | PlaceHelp
  | PlaceCogs
  | PlaceDock
  | Play 
  | DoNothing
  | Win

type CogModel = {
  mutable Cogs : ExtendedSprite<CogData> []
  mutable Targets : ExtendedSprite<CogData> []
  mutable Score : int
  mutable Goal : int
  mutable Found: int []
  mutable State: CogState
  mutable Emitters : PIXI.particles.Emitter []
  mutable Sizes : Size []
  Layers: string list
}

[<RequireQualifiedAccess>]
module IntroductionScreen = 

  type State = 
    | Init 
    | Play 
    | EndAnim 
    | MoveToNextScreen 
    | ByeBye
    | DoNothing

  type Texts = PIXI.Sprite *  PIXI.Sprite

  type CustomSprite = {
    Angle:float
  }

  type Model = {
    mutable State : State
    mutable Texts : Texts option
    Layers: string list
    mutable CogList: ExtendedSprite<CustomSprite> []
    mutable Id: float
  }

type ScreenKind = 
  | GameOfCogs of CogModel option
  | GameOver  
  | Title  of IntroductionScreen.Model option
  | NextScreen of ScreenKind

[<RequireQualifiedAccess>]
module Assets = 
  let mutable sounds = Map.empty<string,PIXI.sound.Sound> 
  let mutable textures = Map.empty<string,PIXI.Texture> 
  let mutable objFiles = Map.empty<string,obj> 

  let addSound name sound = 
    sounds <- sounds.Add(name,sound) 

  let addTexture name texture = 
    textures <- textures.Add(name,texture) 

  let addObj name text = 
    objFiles <- objFiles.Add(name,text)     

  let getTexture name = 
     textures.TryFind name

  let getSound name = 
     sounds.TryFind name

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

        // remove from pixi
        layer.children
          |> Seq.iteri( fun i child -> 
            layer.removeChild( layer.children.[i] ) |> ignore
          )        
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

[<RequireQualifiedAccess>]
module AnimationUtils =
  let XY (target:'t) xFactor yFactor duration elasticity =
    jsOptions<anime.AnimeAnimParams> (fun o ->
      o.elasticity <- !!elasticity
      o.duration <- !!duration
      o.targets <- !!target
      o.Item("x") <- xFactor
      o.Item("y") <- yFactor
    )         