# Yamashita-public

Yamashita専用ライブラリを一部公開しているところです。  

# Contents

(編集中です。バグなどあれば報告ください。)  

## Library

----- [DepthCameras](/Library/Yamashita.DepthCameras)  
----- [KalmanFilter](/Library/Yamashita.KalmanFilter)  
----- [MultiTracker](/Library/Yamashita.MultiTracker)  
----- [Svm](/Library/Yamashita.Svm)  
----- [TcpSocket](/Library/Yamashita.TcpSocket)  
----- [Yolo](/Library/Yamashita.Yolo)  

## Samples

----- [DepthCameras](/Samples/Samples.DepthCameras)  
----- [Socket](/Samples/Samples.Socket)　→　多言語との連携は[SocketSamples](https://github.com/husty530/SocketSamples)へ。  
----- [Tracking](/Samples/Samples.Tracking)  
----- [Yolo-Labeller](/Samples/Samples.Yolo-Labeller)  
----- [Yolo-tiny](/Samples/Samples.Yolo-tiny)  
----- [YoloModel-tiny](/Samples/YoloModel-tiny)  

### tinyじゃないYOLOモデル

* [yolov4.weights](https://github.com/AlexeyAB/darknet/releases/download/darknet_yolo_v3_optimal/yolov4.weights)

### YOLO独自モデルの作り方

tinyじゃない方の説明をしますが、tinyでもほぼ同じです。  
VisualStudioおよびGoogleアカウントを用意してください。  
GoogleDriveとGoogleColabolatory(以下Colab)を使用します。  

#### ラベリング作業  
[Yolo-Labeller](/Samples/Samples.Yolo-Labeller)を使いましょう。  
ディレクトリ下層の"classes.txt"を事前に編集すれば自由にクラスラベルを変えられます。  
キー操作は全部左手におさまるようにしました。(A = Back, D = Next, S = Save, C = Clear, X = Undo)  
  
#### GoogleDriveの編集およびGoogleColabの操作  
  
Colabではドライブをマウントしてデータを扱います。  
以下、手順どおりに進めてください。  
  
1. GoogleColabolatoryにて"[Yolov4.ipynb](/Yolov4.ipynb)"を開き、自分のドライブをマウントする
2. 1つ目のセルでドライブにAlexeyAB/darknetのリポジトリをクローンする
3. ここからドライブ内の作業。"darknet/data/"にフォルダを作成し、作成した画像およびラベルデータをすべて入れる
4. "Makefile" ... GPU,CUDNN,CUDNN_HALF,OPENCVの数値を1に変える
5. "cfg/yolov4-custom.cfg" ... subdivisions=32,max_batches=4000, steps=3200,3600などとし、さらに最後の方にある3つのyoloレイヤーでClasses=(クラス数)に、各yoloレイヤーの直前にあるconvolutionalレイヤーにて"filters=(クラス数+5)×3"に変更する。入力サイズは自由に変えてもよいが32の倍数でないとエラーが出る。これを"yolov4-obj.cfg"と名前をつけて保存。次に最初の方の2つのbatch,subdivisionのコメントアウトを入れ替えて"yolov4-obj-test.cfg"で保存する。
6. "data/coco.names" ... 自分が設定したクラスに書き換えて"obj.names"として保存  
  
7. "cfg/coco.data" ... こんな感じ ↓ にして、"obj.data"と名前をつけ"data/"に保存。
```
classes= (クラス数)
train  = data/train.txt
valid  = data/test.txt
names = data/obj.names
backup = backup/
```  
  
8. "darknet/"に”yolov4.conv.137”を入れる
9. "[process.py](/process.py)"内のフォルダ名を自分がデータを入れたドライブのフォルダ名にして、"darknet/data/"に入れる
10. ここからColab。まず"ランタイムの設定変更"から"GPU"を選択しておく
11. コンパイル → "process.py"(train-testの切り分け) → 学習(最初から)をポチポチするだけ。画面がザーッと流れ続けていたらうまくいっている。  
(2回目以降コンパイルがらみのエラーが出る場合はいったん"darknet.exe"を消してコンパイルし直してください)  
12. 学習モデルは随時"backup/"フォルダに保存される。"chart.png"も保存されるので学習経過を見ることもできる。  
