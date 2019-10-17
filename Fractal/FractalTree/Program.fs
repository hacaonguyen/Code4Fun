//the orginal source is copied from https://github.com/c4fsharp/Dojo-Fractal-Forest
//Thanks Nguyen Bui for review and update the code to enhance and make it look more F# style

open System
open System.Drawing
open System.Windows.Forms


// Create a form to display the graphics
let width, height = 800, 600
let form = new Form(Width = width, Height = height)
let box = new PictureBox(BackColor = Color.White, Dock = DockStyle.Fill)
let image = new Bitmap(width, height)
let graphics = Graphics.FromImage image
//The following line produces higher quality images,
//at the expense of speed. Uncomment it if you want
//more beautiful images, even if it's slower.
//Thanks to https://twitter.com/AlexKozhemiakin for the tip!
//graphics.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality
let brush = new SolidBrush(Color.FromArgb(92, 45, 5))

box.Image <- image
form.Controls.Add box

let pi = Math.PI

// Compute the endpoint of a line
// starting at x, y, going at a certain angle
// for a certain length.
let endpoint point angle length =
    let x, y = point
    x + length * cos angle, y + length * sin angle

let flip x = (float height) - x

// Utility function: draw a line of given width,
// starting from point(x, y)
// going at a certain angle, for a certain length.
let drawLine (target : Graphics) (brush : Brush)
             point (angle : float) (length : float) (width : float) =
    let x, y = point
    let x', y' = endpoint point angle length
    let origin = PointF(single x, single(y |> flip))
    let destination = PointF(single x', single(y' |> flip))
    let pen = new Pen(brush, single width)
    target.DrawLine(pen, origin, destination)

let random = Random().NextDouble
let randomD d = d * random()
let randomI = Random().Next

[<Struct>]
type RGB = {R: int; G: int; B: int}

let argb (rgb: RGB) = Color.FromArgb(rgb.R, rgb.G, rgb.B)

let drawLeaf point =
    let green = {R = randomI 100; G = 156 + randomI 100; B = randomI 100}
    let pink = {R = 176 + randomI 80; G = 127 + randomI 50; B = 163 + randomI 50}
    let drawFlower = false //random() < 0.5
    let color, len, wid =
        if drawFlower then pink, 2.0 + randomD 3.0, 2.0 + randomD 3.0
        else green, 2. + randomD 3.0, 1. + random()
    use br = new SolidBrush(argb color)
    let angle = pi*random()
    drawLine graphics br point angle len wid

let drawLeaves point angle length =
    let point' = endpoint point angle length
    let somewhere = length * (0.25 + random()/2.)
    let point'' = endpoint point angle somewhere
    let n1 = 2 + randomI 4
    let n2 = 2 + randomI 4
    for i = 1 to n1 do drawLeaf point'
    for i = 1 to n2 do drawLeaf point''

let draw point = drawLine graphics brush point

// Now... your turn to draw

let maxDepth = 9

let rec branch (curDepth:int) point (ang0 : float) (len0 : float) (wid0 : float) =
    // we draw the current segment

    //add random
    let ang = ang0 - 0.1 + random() / 5.0
    let len = len0 * (0.9 + random() / 5.0)
    let wid = wid0 * (0.8 + random() / 2.5)

    draw point ang len wid
    if curDepth > 4 then drawLeaves point ang len
    // if max depth hasn't been reached yet,
    // we create 2-3 branches and keep going
    if curDepth > maxDepth || curDepth > 5 && random() < 0.3
    then ignore ()
    else
        // compute end coordinates of current segment
        let point' = endpoint point ang len
        let recur = branch (curDepth + 1) point'

        let goLeft = random() < 0.7
        let goCenter = not goLeft || random() < 0.7
        let goRight = not goLeft || not goCenter || random() < 0.7

        // go left
        if goLeft || curDepth > 4 then
            recur (ang + 0.4) (len * 0.8) (wid * 0.6)

        // go center
        if goCenter && curDepth < 5 then
            recur ang (len * 0.8) (wid * 0.7)

        // go right
        if goRight || curDepth > 4 then
            recur (ang - 0.4) (len * 0.8) (wid * 0.6)

let drawTree () =
    graphics.Clear Color.Transparent
    box.Invalidate()
    branch 0 (400., 50.) (pi/2.0) 100.0 12.0

box.Click.Add (fun _ -> drawTree())
drawTree()
form.ShowDialog() |> ignore