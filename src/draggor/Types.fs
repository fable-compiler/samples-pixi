module Types 

open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi

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

type CogModel = {
  mutable Cogs : ExtendedSprite<CogData> []
  mutable Targets : ExtendedSprite<CogData> []
  mutable Score : int
  mutable Goal : int
  mutable Found: int []
  mutable State: CogState
  mutable Emitters : PIXI.particles.Emitter []
  mutable Sizes : Size []
}

[<RequireQualifiedAccess>]
module IntroductionScreen = 

  type State = 
    | Init 
    | Play 
    | NextScreen 
    | DoNothing

  type Msg = 
    | OnClick
    | Done

  type Model = {
    mutable State : State
    mutable Msg : Msg option
  }

[<RequireQualifiedAccess>]
type ScreenKind = 
  | GameOfCogs of CogModel
  | GameOver  
  | Introduction  of IntroductionScreen.Model
  | NextScreen of ScreenKind

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
