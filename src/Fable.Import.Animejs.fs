namespace rec Fable.Import.Animejs

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS
open Fable.Import.Browser

type FunctionBasedParamter =
    Func<HTMLElement, float, float, float>

and AnimeCallbackFunction =
    anime.AnimeInstance -> unit

and AnimeTarget =
    obj

module anime =
    type EasingOptions =
        obj

    and [<StringEnum>] DirectionOptions =
        | Reverse | Alternate | Normal

    and [<AllowNullLiteral>] AnimeInstanceParams =
        abstract loop: U2<float, bool> option with get, set
        abstract autoplay: bool option with get, set
        abstract direction: U2<DirectionOptions, string> option with get, set
        abstract ``begin``: AnimeCallbackFunction option with get, set
        abstract run: AnimeCallbackFunction option with get, set
        abstract update: AnimeCallbackFunction option with get, set
        abstract complete: AnimeCallbackFunction option with get, set

    and [<AllowNullLiteral>] AnimeAnimParams =
        abstract targets: U2<AnimeTarget, ReadonlyArray<AnimeTarget>> with get, set
        abstract duration: U2<float, FunctionBasedParamter> option with get, set
        abstract delay: U2<float, FunctionBasedParamter> option with get, set
        abstract elasticity: U2<float, FunctionBasedParamter> option with get, set
        abstract round: U3<float, bool, FunctionBasedParamter> option with get, set
        abstract easing: U3<EasingOptions, string, ReadonlyArray<float>> option with get, set
        abstract ``begin``: AnimeCallbackFunction option with get, set
        abstract run: AnimeCallbackFunction option with get, set
        abstract update: AnimeCallbackFunction option with get, set
        abstract complete: AnimeCallbackFunction option with get, set
        [<Emit("$0[$1]{{=$2}}")>] abstract Item: AnyAnimatedProperty: string -> obj with get, set

    and [<AllowNullLiteral>] AnimeParams =
        inherit AnimeInstanceParams
        inherit AnimeAnimParams


    and [<AllowNullLiteral>] AnimeInstance =
        abstract began: bool with get, set
        abstract paused: bool with get, set
        abstract completed: bool with get, set
        abstract finished: Promise<unit> with get, set
        abstract ``begin``: AnimeCallbackFunction with get, set
        abstract run: AnimeCallbackFunction with get, set
        abstract update: AnimeCallbackFunction with get, set
        abstract complete: AnimeCallbackFunction with get, set
        abstract autoplay: bool with get, set
        abstract currentTime: float with get, set
        abstract delay: float with get, set
        abstract direction: string with get, set
        abstract duration: float with get, set
        abstract loop: U2<float, bool> with get, set
        abstract offset: float with get, set
        abstract progress: float with get, set
        abstract remaining: float with get, set
        abstract reversed: bool with get, set
        abstract animatables: ReadonlyArray<obj> with get, set
        abstract animations: ReadonlyArray<obj> with get, set
        abstract play: unit -> unit
        abstract pause: unit -> unit
        abstract restart: unit -> unit
        abstract reverse: unit -> unit
        abstract seek: time: float -> unit

    and [<AllowNullLiteral>] AnimeTimelineAnimParams =
        inherit AnimeAnimParams
        abstract offset: U3<float, string, FunctionBasedParamter> with get, set

    and [<AllowNullLiteral>] AnimeTimelineInstance =
        inherit AnimeInstance
        abstract add: ``params``: AnimeAnimParams -> AnimeTimelineInstance

    and [<AllowNullLiteral>] easingsType =
        [<Emit("$0[$1]{{=$2}}")>] abstract Item: EasingFunction: string -> Func<float, obj> with get, set

    type [<Import("*","anime")>] Globals =
        static member speed with get(): float = jsNative and set(v: float): unit = jsNative
        static member running with get(): ResizeArray<AnimeInstance> = jsNative and set(v: ResizeArray<AnimeInstance>): unit = jsNative
        static member easings with get(): easingsType = jsNative and set(v: easingsType): unit = jsNative
        static member remove(targets: U2<AnimeTarget, ReadonlyArray<AnimeTarget>>): unit = jsNative
        static member getValue(targets: AnimeTarget, prop: string): U2<string, float> = jsNative
        static member path(path: U4<string, HTMLElement, SVGElement, obj>, ?percent: float): string -> obj = jsNative
        static member setDashoffset(el: U3<HTMLElement, SVGElement, obj>): float = jsNative
        static member bezier(x1: float, y1: float, x2: float, y2: float): Func<float, float> = jsNative
        static member timeline(?``params``: U2<AnimeInstanceParams, ReadonlyArray<AnimeInstance>>): AnimeTimelineInstance = jsNative
        static member random(min: float, max: float): float = jsNative


