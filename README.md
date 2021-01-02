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
----- [Socket](/Samples/Samples.Socket) ... [SocketSamples](https://github.com/husty530/SocketSamples)で他言語(Node.js, C++, Python)との連携やってます。  
----- [Tracking](/Samples/Samples.Tracking)  
----- [Yolo-Labeller](/Samples/Samples.Yolo-Labeller) ... Yoloの学習データを作るラベリングツール  
----- [Yolo-tiny](/Samples/Samples.Yolo-tiny)  
----- [YoloModel-tiny](/Samples/YoloModel-tiny) ... tinyじゃない方のモデル・独自モデルの作り方は[YoloSamples](https://github.com/husty530/YoloSamples)へ。  

### その他
[YAMASHITAの玩具箱](https://qiita.com/husty530/items/3b4e05733e2908b1ad1d) ... 遊び半分のものはこちらに。相互リンクしてます。  

# YOLOの仕様
<使用例>  
```
var detector = new Yolo(cfg, names, weights, new Size(640, 480), DrawingMode.Rectangle, 0.5f, 0.3f);  
detector.Run(ref frame, out var results);  
```  
  
出力のYoloResultsクラスはそれぞれList型のLabels, Confidences, Centers, Sizesに加え、インデクサを実装、IEnumerable<>・IEnumerator<>を継承しているのでforeachとLinqが使えます。  
これの何がありがたいかというと、  
  
```
detector.Run(ref frame, out var results);  
SomeFunction(results.Centers);  
```
↑↑↑　のように各要素のListを取り出せるほか、  
  
```  
detector.Run(ref frame, out var results);  
for (int i = 0; i < results.Count; i++)  
{  
　　// 次のコードはどちらも同じ意味  
　　SomeFunction(results.Centers[i]);  
　　SomeFunction(results[i].Center);  
}  
```  
↑↑↑　for文でのアクセスの仕方が自由になり、  
```  
detector.Run(ref frame, out var results);  
var list = new List<(string, Point, Size)>();  
foreach (var r in results)  
{  
　　list.Add((r.Label, r.Center, r.Size));  
}  
SomeFunction(list);  
```  
↑↑↑　foreach文内でも要素を取り出せたり、  
  
```  
detector.Run(ref frame, out var results);
var list = results  
　　.Select(r => (r.Label, r.Center, r.Size))  
　　.ToList();  
SomeFunction(list);  
``` 
↑↑↑　なんならLinq使えばforeachすら要らなかったり、  
  
```
detector.Run(ref frame, out var results);  
var list = results  
　　.Where(r => r.Label == "pumpkin")  
　　.Where(r => r.Confidence > 0.8)
　　.Select(r => r.Center)  
　　.ToList();  
SomeFunction(list);  
```  
↑↑↑　条件検索をかけることが可能になります。  
