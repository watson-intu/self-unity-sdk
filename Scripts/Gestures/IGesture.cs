/**
* Copyright 2016 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using System;
using System.Collections;

namespace IBM.Watson.Self.Gestures
{
    public delegate void OnGestureDone(IGesture a_Gesture, bool a_bError);

    //! This is the base class for a self gesture. 
    public interface IGesture
    {
        //! The ID of this gesture.
        string GetGestureId();
        //! return an ID unique to this instance
        string GetInstanceId();
        //! Initialize this gesture object, returns false if gesture can't be initialized
        bool OnStart();
        //! Shutdown this gesture object.
        bool OnStop();
        //! Execute this gesture, the provided callback should be invoked when the gesture is complete.
        bool Execute(OnGestureDone a_Callback, IDictionary a_Params);
        //! Abort this gesture, if true is returned then abort succeeded and callback will NOT be invoked.
        bool Abort();
    }
}
