using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.SIL
{
    public enum EleTagResponseState
    {
        NONE,
        TAKE_CACHE,
        TAKE_PING,
        TAKE_SHOW,
        TAKE_SHOW_ERROR,
        TAKE_ACK,
        TAKE_COMPLETE,
        TAKE_ACK_ERROR,
        ADD_CACHE,
        ADD_PING,
        ADD_SHOW,
        ADD_SHOW_ERROR,
        ADD_ACK,
        ADD_COMPLETE,
        ADD_ACK_ERROR
        //ADDRESS,
        //TAKING,
        //TAKE_QUERY,
        //TAKED,
        //ADDING,
        //ADD_QUERY,
        //ADDED,
        //TAKE_ACK_NO_MAPPING,
        //ADD_ACK_NO_MAPPING,
        //QUERY_ACK_NO_MAPPING,
        //TAKING_ERROR,
        //TAKED_ERROR,
        //ADDING_ERROR,
        //ADDED_ERROR
    }
}
