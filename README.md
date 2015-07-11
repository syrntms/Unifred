Unifred
====

## Overview
このアセットはUnityのエディター上でSpotlightやAlfredのような一時的なユーザー入力を受け付けるウィンドウを提供するアセットです  

## Usage
Window操作  
* 決定  
	* Enter  
* キャンセル  
	* Esc  
* 選択列の移動  
	* Up / Down / Tab / Shift + Tabキー  
* 複数選択  
	* Up / Down キーで移動の際にShift  
  
  
  
デフォルト機能  
* command + y  
	* 選択中のゲームオブジェクトのスクリプトからメソッド呼び出し  
* command + [  
	* 選択中のゲームオブジェクトのコンポーネントをコピー  
* command + ]  
	* コピーしたコンポーネントを選択中のゲームオブジェクトへペースト  
* command + g  
	* Hierarchy内検索  
* command + t  
	* Project内検索  
* command + ^  
	* Unifredの履歴から検索  

## Caution
	OSX環境のメソッド呼び出しでエラーが出る場合はこちらを試してみてください
	sudo ln -s /Applications/Unity/MonoDevelop.app/Contents/Frameworks/Mono.framework/Versions/{Version}/bin/gmcs /usr/bin
