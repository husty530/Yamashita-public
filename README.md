# Yamashita-public 

# Contents
(編集中です。バグなどあれば報告ください。)  

### Library
----- [DepthCameras](/Library/Yamashita.DepthCameras)  
----- [KalmanFilter](/Library/Yamashita.KalmanFilter)  
----- [MultiTracker](/Library/Yamashita.MultiTracker)  
----- [Svm](/Library/Yamashita.Svm)  
----- [TcpSocket](/Library/Yamashita.TcpSocket)  
----- [Yolo](/Library/Yamashita.Yolo)  

### Samples
----- [DepthCameras](/Samples/Samples.DepthCameras)  
----- [Socket](/Samples/Samples.Socket) ... 多言語との連携は[SocketSamples](https://github.com/husty530/SocketSamples)へ。  
----- [Tracking](/Samples/Samples.Tracking)  
----- [Yolo-Labeller](/Samples/Samples.Yolo-Labeller) ... Yoloの学習データを作るラベリングツール  
----- [Yolo-tiny](/Samples/Samples.Yolo-tiny)  
----- [YoloModel-tiny](/Samples/YoloModel-tiny) ... tinyじゃない方のモデル・独自モデルの作り方は[YoloSamples](https://github.com/husty530/YoloSamples)で。  

### その他
[YAMAHITAの玩具箱](https://qiita.com/husty530/items/3b4e05733e2908b1ad1d) ... 遊び半分のものはこちらに。相互リンクしてます。  

# YOLOの仕様
<使用例>  
```
var detector = new Yolo(cfg, names, weights, new Size(640, 480), DrawingMode.Point, 0.5f, 0.3f); //DrawingModeはOff, Point, Rectangle  
detector.Run(ref frame, out var yolo); //frameはMat  
```  
出力のYoloResultsクラスはそれぞれList型のLabels, Confidences, Centers, Sizesに加え、インデクサを実装、IEnumerable<>・IEnumerator<>を継承しているのでfor文・foreach文が使えます。  
これの何がありがたいかというと、  
```
detector.Run(ref frame, out var yolo);  
AnyFunction(yolo.Centers);  
```
のように中心座標のリストだけを取り出したり、  
```  
detector.Run(ref frame, out var yolo);  
var list = new List<(string, Point, Size)>();  
foreach (var y in yolo) list.Add((y.Label, y.Center, y.Size));  
```  
foreach文で要素を取り出せたり、  
```  
detector.Run(ref frame, out var yolo);
var list = yolo.Select(y => (y.Label, y.Center, y.Size)).ToList();
``` 
なんならLinq使えばforeachすら要らなかったり、  
```
detector.Run(ref frame, out var yolo);  
var centers = yolo  
　　.Where(y => y.Label == "pumpkin")  
　　.Where(y => y.Confidence > 0.8)
　　.Select(y => y.Center)  
　　.ToList();  
```  
条件検索をかけることも可能になります。  
