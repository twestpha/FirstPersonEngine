//##################################################################################################
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//##################################################################################################

using UnityEngine;
using System;
using System.Collections.Generic;

//##################################################################################################
// Delegate List
// An interface
//##################################################################################################
public class DelegateList<T> where T : Delegate {

    private List<T> delegates;

    public void Register(T d){
        if(delegates == null){
            delegates = new List<T>();
        }

        delegates.Add(d);
    }

    public void Invoke(params System.Object[] args){
        if(delegates != null){
            foreach(T d in delegates){
                d.DynamicInvoke(args);
            }
        }
    }
};
