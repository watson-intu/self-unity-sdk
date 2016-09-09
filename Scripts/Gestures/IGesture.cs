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
    //! This is the base class for a self gesture. 
    public abstract class IGesture
    {
        #region Public Types
        public delegate void GestureDone(IGesture a_Gesture, bool a_bError);
        #endregion

        #region Interface
        //! The ID of this gesture.
        public abstract string GetGestureId();
        //! Initialize this gesture object, returns false if gesture can't be initialized
        public abstract bool OnStart();
        //! Shutdown this gesture object.
        public abstract bool OnStop();
        //! Execute this gesture, the provided callback should be invoked when the gesture is complete.
        public abstract bool Execute(GestureDone a_Callback, IDictionary a_Params);
        //! Abort this gesture, if true is returned then abort succeeded and callback will NOT be invoked.
        public abstract bool Abort();
        #endregion
    }
}
