let gmeAPI = new WebGMEAPI();
var document;
var userSig;
var enterRoomCompleteDelegate;

function WebGMEInit(sdkAppId,openId) {
    WebGMEAPI.Param.document = document;
    WebGMEAPI.Param.sdkAppId = sdkAppId;
    WebGMEAPI.Param.openId = openId;
    console.log('WebGMEInit, sdkAppID:' + sdkAppId+ " openId: " + openId);
	gmeAPI.Init(document,sdkAppId,openId);
	//gmeAPI.SetTMGDelegate(onEvent);
}

function toUint8Arr(str) {
            const buffer = [];
            for (let i of str) {
                const _code = i.charCodeAt(0);
                if (_code < 0x80) {
                    buffer.push(_code);
                } else if (_code < 0x800) {
                    buffer.push(0xc0 + (_code >> 6));
                    buffer.push(0x80 + (_code & 0x3f));
                } else if (_code < 0x10000) {
                    buffer.push(0xe0 + (_code >> 12));
                    buffer.push(0x80 + (_code >> 6 & 0x3f));
                    buffer.push(0x80 + (_code & 0x3f));
                }
            }
            return Uint8Array.from(buffer);
}

function GenAuthBuffer(appId,roomID,openId,authKey,jsauthBuffer,authBufferLen) {
    console.log('appId:' + appId + ' roomID:' + roomID +' openId:' + openId + ' key:' +authKey + 'authBufferLen: ' + authBufferLen);
	if(roomID == ""){
		roomID = '200';
	}
    let authBuffer = new AuthBufferService(appId, roomID, openId, authKey);
    userSig = authBuffer.getSignature();
	authBufferLen = userSig.length;
	//console.log('jsauthBuffer:'+userSig.length);
	//for(var num = 0;num<userSig.length;num++){
    //    jsauthBuffer[num]= userSig[num]>>> 0;//1,2,3,4,5,6,7,8,9
	//	console.log(jsauthBuffer[num]);
	//}
    console.log('userSig:' + JSON.stringify(userSig));
	return userSig.length;
}

function EnterRoom(roomID,roomType,authBuffer) {
	if(authBuffer.length >= 5){
		userSig = authBuffer;
		console.log('entering room, the authBuffer should be get from the remote end');
	}
	console.log('EnterRoom: roomID:' + roomID + "userSig"+userSig);
	return gmeAPI.EnterRoom(roomID,1,userSig);
}

function onEvent (eventType, result) {
    console.log('onEvent: eventTpye = ' + eventType + 'result = '+ JSON.stringify(result));
};
	
function initFromHtml(_document) {
    document = _document;
}

function SetDelegate(enterRoomComplete) {
    enterRoomCompleteDelegate = enterRoomComplete;
}

function SetRangeAudioTeamID (teamID){
	let rst = gmeAPI.SetRangeAudioTeamID(teamID);
	return rst;
}

function SetRangeAudioMode(audioMode) {
	let rst = gmeAPI.SetRangeAudioMode(audioMode);
	return rst;
}

function UpdateSelfPosition(x, y, z) {
	let selfPosition = {x, y, z};
	let rst = gmeAPI.UpdateSelfPosition(selfPosition, null, null, null);
	return rst;
}

function UpdateAudioRecvRange(range) {
	let rst = gmeAPI.UpdateAudioRecvRange(range);
	return rst;
}

