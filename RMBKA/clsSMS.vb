Imports System
Imports System.IO
Imports System.Resources
Imports System.Net
Imports System.IO.FileStream
Imports System.IO.File
Imports System.Data.OleDb
Imports System.Xml
Imports System.Configuration

Public Class ClsCorrespondSMS

    Public Function SendSMS_LikeToLike(ByVal Message() As String, ByVal DestinationAddress() As String, ByVal UserName As String, ByVal Password As String, ByVal arrSenderNumber() As String, ByVal IPAddress As String, ByVal Company As String, ByVal BatchID As String) As String()

        Dim RetValue(1) As String
        RetValue(0) = "False"
        RetValue(1) = 0

        If Message.Length <> DestinationAddress.Length Then
            RetValue(1) = "Incorrect array size for Messages and Destinations"
            Return RetValue
        End If

        Dim DestSize As Integer = DestinationAddress.Length

        Try

            Dim txt As String = ""
            Dim ackType As String = "Full"
            txt = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
            txt = txt & "<!DOCTYPE smsBatch PUBLIC ""-//PERVASIVE//DTD CPAS 1.0//EN"" ""http://www.ubicomp.ir/dtd/Cpas.dtd"">" & vbCrLf
            txt = txt & "<smsBatch  ackType=""" & ackType & """ company=""" & Company & """ batchID=""" & BatchID & """>" & vbCrLf

            For i = 0 To DestSize - 1

                Dim strMessage As String = Message(i)
                Dim strDestinationAddress As String = DestinationAddress(i)
                Dim IsFarsi As Boolean = Language(strMessage)

                If (IsFarsi) Then
                    strMessage = C2Unicode(strMessage)
                End If

                txt = txt & "<sms" & IIf(IsFarsi, " binary=""true"" dcs=""8""", " binary=""false"" dcs=""0""") & ">" & vbCrLf
                txt = txt & "<origAddr><![CDATA[" & arrSenderNumber(i) & "]]></origAddr>" & vbCrLf
                txt = txt & "<destAddr><![CDATA[" & strDestinationAddress & "]]></destAddr>" & vbCrLf
                txt = txt & "<message><![CDATA[" & strMessage & "]]></message>" & vbCrLf
                txt = txt & "</sms>"
            Next

            txt = txt & vbCrLf & "</smsBatch>"

            Dim DataToPost As String = txt
            Dim WebRequest As WebRequest
            Dim RequestStream As Stream
            Dim StreamWriter As StreamWriter
            Dim WebResponse As WebResponse
            Dim ResponseStream As Stream
            Dim StreamReader As StreamReader

            WebRequest = WebRequest.Create(IPAddress)
            WebRequest.Method = "POST"
            WebRequest.ContentType = "text/xml"
            WebRequest.Headers.Add("authorization", "Basic " & Base64Encode(UserName & ":" & Password))
            RequestStream = WebRequest.GetRequestStream()
            StreamWriter = New StreamWriter(RequestStream)
            StreamWriter.Write(DataToPost)
            StreamWriter.Flush()
            StreamWriter.Close()
            RequestStream.Close()

            WebResponse = WebRequest.GetResponse()
            ResponseStream = WebResponse.GetResponseStream()
            StreamReader = New StreamReader(ResponseStream)
            Dim dataToReceive As String = StreamReader.ReadToEnd()
            StreamReader.Close()
            ResponseStream.Close()
            WebResponse.Close()

            If InStr(dataToReceive, "CHECK_OK", CompareMethod.Text) > 0 Then
                RetValue(0) = "CHECK_OK"
                RetValue(1) = BatchID
            Else
                Try
                    Dim Sindex, EIndex, MSG
                    Sindex = InStr(dataToReceive, "CDATA[", CompareMethod.Text)
                    EIndex = InStr(dataToReceive, "]]", CompareMethod.Text)
                    MSG = dataToReceive.Substring(Sindex + "CDATA[".Length - 1, EIndex - Sindex - "CDATA[".Length)
                    RetValue(1) = MSG
                    Return RetValue
                Catch ex As Exception
                    RetValue(1) = ex.Message.ToString()
                    Return RetValue
                End Try
            End If

        Catch ex As Exception
            RetValue(1) = ex.Message.ToString()
            Return RetValue
        End Try

        Return RetValue

    End Function

    'Public Function correctNumber(ByVal uNumber As String) As String
    '    Dim ret As String = Trim(uNumber)
    '    If ret.Substring(0, 4) = "0098" Then ret = ret.Remove(0, 4)
    '    If ret.Substring(0, 3) = "098" Then ret = ret.Remove(0, 3)
    '    If ret.Substring(0, 3) = "+98" Then ret = ret.Remove(0, 3)
    '    If ret.Substring(0, 2) = "98" Then ret = ret.Remove(0, 2)
    '    If ret.Substring(0, 1) = "0" Then ret = ret.Remove(0, 1)
    '    'If ret.Substring(0, 2) = "91" Then ret = "0" + ret
    '    Return "+98" & ret
    'End Function

    Public Function C2Unicode(ByVal uMessage As String) As String

        Dim i As Integer
        Dim preUnicode_Number As Integer
        Dim preUnicode As String
        Dim ret As String = ""
        Dim strHex As String = ""
        For i = 0 To uMessage.Length - 1
            preUnicode_Number = 4 - String.Format("{0:X}", AscW(uMessage.Substring(i, 1))).Length
            preUnicode = String.Format("{0:D" & preUnicode_Number.ToString() & "}", 0)
            strHex = preUnicode & String.Format("{0:X}", AscW(uMessage.Substring(i, 1)))
            If strHex.Length = 4 Then
                ret &= strHex
            End If
        Next
        Return ret

    End Function

    Private Function MyASC(ByVal OneChar As String) As Integer
        If OneChar = "" Then MyASC = 0 Else MyASC = Asc(OneChar)
    End Function

    Private Function Base64Encode(ByVal inData As String) As String
        'rfc1521
        '2001 Antonin Foller, Motobit Software, http://Motobit.cz
        Const Base64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        Dim sOut As String = ""
        Dim I As Integer

        'For each group of 3 bytes
        For I = 1 To Len(inData) Step 3
            Dim nGroup, pOut As String

            'Create one long from this 3 bytes.
            nGroup = &H10000 * Asc(Mid(inData, I, 1)) + _
              &H100 * MyASC(Mid(inData, I + 1, 1)) + MyASC(Mid(inData, I + 2, 1))

            'Oct splits the long To 8 groups with 3 bits
            nGroup = Oct(nGroup)

            'Add leading zeros
            nGroup = StrDup(8 - Len(nGroup), "0") & nGroup

            'Convert To base64
            pOut = Mid(Base64, CLng("&o" & Mid(nGroup, 1, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 3, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 5, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 7, 2)) + 1, 1)


            'Add the part To OutPut string
            sOut = sOut + pOut

            'Add a new line For Each 76 chars In dest (76*3/4 = 57)
            'If (I + 2) Mod 57 = 0 Then sOut = sOut + vbCrLf
        Next
        Select Case Len(inData) Mod 3
            Case 1 '8 bit final
                'sOut = Left(sOut, Len(sOut) - 2) + "=="
                sOut = sOut.Substring(0, Len(sOut) - 2) + "=="



            Case 2 '16 bit final
                'sOut = Left(sOut, Len(sOut) - 1) + "="
                sOut = sOut.Substring(0, Len(sOut) - 1) + "="
        End Select
        Base64Encode = sOut
    End Function

    Public Function Language(ByVal Datas As String) As Boolean
        Dim Farsi As Boolean = False
        Dim intMessageLength As Integer = 0
        Dim unicodeBytes As Byte() = Text.UnicodeEncoding.Unicode.GetBytes(Datas)
        For i As Integer = 1 To unicodeBytes.GetLength(0) - 1 Step 2
            If unicodeBytes(i) <> 0 Then
                intMessageLength = 70
                Exit For
            Else
                intMessageLength = 160
            End If
        Next i
        Farsi = If(intMessageLength = 70, True, False)
        Return Farsi

    End Function

End Class

''' <remarks></remarks>
Public Class SMSStatus
    Public Structure STC_SMSStatus
        Public ReceiveNumber As String
        Public DeliveryStatus As String
    End Structure
    Public Function StatusSMS(ByVal userName As String, ByVal password As String, ByVal IP_Send As String, ByVal Company As String, ByVal Batch As String) As STC_SMSStatus()
        Try
            Dim S() As String
            Dim StrXML As String
            ''''''''''''''''''''' Read Single SMS Status
            StrXML = "<?xml version=""1.0""?>" & vbCrLf
            StrXML = StrXML & "<!DOCTYPE smsStatusPoll PUBLIC ""-//PERVASIVE//DTD CPAS 1.0//EN"" ""http://www.pervasive.ir/dtd/Cpas.dtd"" []>" & vbCrLf
            StrXML = StrXML & "<smsStatusPoll company=""" + Company + """ >" & vbCrLf
            StrXML = StrXML & "<batch batchID=""" + Batch + """ />" & vbCrLf
            StrXML = StrXML & "</smsStatusPoll>"
            ''''''''''''''''''''''''''''''''''''''''''''''' 
            ' Here's the data I'll be sending.  It's the same format
            ' you'd use in a querysting and you should URLEncode any
            ' data that may need it.
            Dim vDataToPost As String = StrXML
            Dim vWebRequest As WebRequest
            Dim vRequestStream As Stream

            Dim vStreamWriter As StreamWriter
            Dim vWebResponse As WebResponse
            Dim vResponseStream As Stream
            Dim vStreamReader As StreamReader
            ' Create a new WebRequest which targets that page with
            ' which we want to interact.
            vWebRequest = WebRequest.Create(IP_Send)
            ' Set the method to "POST" and the content type so the
            ' server knows to expect form data in the body of the
            ' request.
            vWebRequest.Method = "POST"
            vWebRequest.ContentType = "text/xml"
            vWebRequest.Headers.Add("authorization", "Basic " & Base64Encode(userName & ":" & password))
            ' Get a handle on the Stream of data we're sending to
            ' the remote server, connect a StreamWriter to it, and
            ' write our data to the Stream using the StreamWriter.
            vRequestStream = vWebRequest.GetRequestStream()
            vStreamWriter = New StreamWriter(vRequestStream)
            vStreamWriter.Write(vDataToPost)
            vStreamWriter.Flush()
            vStreamWriter.Close()
            vRequestStream.Close()
            ' Get the response from the remote server.
            vWebResponse = vWebRequest.GetResponse()
            ' Get the server's response status?
            ' Just like when we sent the data, we'll get a reference
            ' to the response Stream, connect a StreamReader to the
            ' Stream and use the reader to actually read the reply.
            vResponseStream = vWebResponse.GetResponseStream()
            vStreamReader = New StreamReader(vResponseStream)
            StrXML = vStreamReader.ReadToEnd()
            vStreamReader.Close()
            vResponseStream.Close()
            ' Close the WebResponse
            vWebResponse.Close()
            Dim ParseXML As String
            ParseXML = StrXML.Substring(0, 43) + StrXML.Substring(153)

            Dim XmlReader As New Xml.XmlDocument
            XmlReader.XmlResolver = Nothing
            XmlReader.InnerXml = ParseXML
            Dim stc_st As STC_SMSStatus() = Nothing
            Dim ndRoot = XmlReader.DocumentElement

            Dim i As Integer = 0
            For Each ndBatch As System.Xml.XmlNode In ndRoot.ChildNodes
                For Each ndSMS As System.Xml.XmlNode In ndBatch.ChildNodes
                    If stc_st Is Nothing Then
                        ReDim Preserve stc_st(0)
                    Else
                        ReDim Preserve stc_st(stc_st.GetLength(0))
                    End If
                    stc_st(stc_st.GetLength(0) - 1).ReceiveNumber = ndSMS.ChildNodes(0).InnerText
                    stc_st(stc_st.GetLength(0) - 1).DeliveryStatus = ndSMS.ChildNodes(1).InnerText
                Next
                i = i + 1
            Next
            Return stc_st
        Catch ex As Exception
            Throw
        End Try
    End Function
    Public Function ReadXML(ByVal txt As String, ByVal Number As String) As Array
        Try
            Dim StrRecivedMessage(1) As String
            Dim I As Integer
            Dim settings As XmlReaderSettings = New XmlReaderSettings()
            settings.ProhibitDtd = False
            settings.XmlResolver = Nothing
            Dim reader As System.Xml.XmlReader = System.Xml.XmlReader.Create(New System.IO.StringReader(txt), settings)
            Dim Element As String = ""
            Dim StrSMS As String = ""
            Dim StrMessage As String = ""
            Dim strTmp = ""
            Dim blnFindNo As Boolean = False
            I = 0
            Do While (reader.Read())
                Select Case reader.NodeType
                    Case XmlNodeType.Element 'Display beginning of element.
                        Element = reader.Name
                        If reader.HasAttributes Then
                            While reader.MoveToNextAttribute()
                                'Display attribute name and value.
                                If Element = "batch" Then
                                End If
                            End While
                        End If
                    Case XmlNodeType.Text 'Display the text in each element.
                        If Element = "status" Then
                            I = I
                            StrRecivedMessage(I) = reader.Value
                        End If

                        If Element = "time" And blnFindNo = True Then
                            I = I + 1
                            StrRecivedMessage(I) = reader.Value
                            Exit Do
                        End If
                    Case XmlNodeType.CDATA  'Display end of element.
                        If Element = "destAddr" Then
                            If reader.Value <> "+98" & Number.Substring(1) Then
                                Continue Do
                            Else
                                blnFindNo = True
                            End If
                        End If
                        If Element = "origAddr" Then
                            I = I + 1
                            StrRecivedMessage(I) = reader.Value

                        ElseIf Element = "message" Then
                            strTmp = reader.Value
                        End If
                End Select
            Loop
            Return StrRecivedMessage
        Catch ex As Exception
            Throw
        End Try
    End Function
    Private Function Base64Encode(ByVal inData As String) As String
        '2001 Antonin Foller, Motobit Software, http://Motobit.cz
        Const Base64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        Dim sOut As String = ""
        Dim I As Integer
        'For each group of 3 bytes
        For I = 1 To Len(inData) Step 3
            Dim nGroup, pOut As String
            'Create one long from this 3 bytes.
            nGroup = &H10000 * Asc(Mid(inData, I, 1)) + _
              &H100 * MyASC(Mid(inData, I + 1, 1)) + MyASC(Mid(inData, I + 2, 1))
            'Oct splits the long To 8 groups with 3 bits
            nGroup = Oct(nGroup)
            'Add leading zeros
            nGroup = StrDup(8 - Len(nGroup), "0") & nGroup
            'Convert To base64
            pOut = Mid(Base64, CLng("&o" & Mid(nGroup, 1, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 3, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 5, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 7, 2)) + 1, 1)
            'Add the part To OutPut string
            sOut = sOut + pOut
        Next
        Select Case Len(inData) Mod 3
            Case 1
                sOut = sOut.Substring(0, Len(sOut) - 2) + "=="
            Case 2
                sOut = sOut.Substring(0, Len(sOut) - 1) + "="
        End Select
        Base64Encode = sOut
    End Function
    Private Function MyASC(ByVal OneChar As String) As Integer
        If OneChar = "" Then MyASC = 0 Else MyASC = Asc(OneChar)
    End Function
    Public Function DecodeUCS2(ByVal Content As String) As String
        On Error GoTo errHandler
        Dim hextext As String = Content
        Dim i As Long, ret As String
        ret = ""
        For i = 1 To Len(hextext) Step 4
            ret = ret & ChrW(Val("&h" & Mid(hextext, i, 4)))
        Next
        Return ret
        Exit Function
errHandler:
        Return hextext
    End Function
    Public Function DecodePersianNumber(ByVal PersianNumber As String) As String
        Dim EngNum As String = ""
        For i = 0 To PersianNumber.Length - 1
            Dim number As String = PersianNumber.Substring(i, 1)
            Select Case number
                Case "۰"
                    EngNum &= "0"
                Case "۱"
                    EngNum &= "1"
                Case "۲"
                    EngNum &= "2"
                Case "۳"
                    EngNum &= "3"
                Case "۴"
                    EngNum &= "4"
                Case "۵"
                    EngNum &= "5"
                Case "۶"
                    EngNum &= "6"
                Case "۷"
                    EngNum &= "7"
                Case "۸"
                    EngNum &= "8"
                Case "۹"
                    EngNum &= "9"
            End Select
        Next
        Return EngNum
    End Function
End Class

Public Class SMSReceive

    Public Structure STC_SMSReceive
        Public SenderNumber As String
        Public SourceNumber As String
        Public Message As String
        Public ReceiveDate As Date
        Public RecieveBatchID As String
    End Structure

    Public Function ExtendReceiveSMS(ByVal vUser As String, ByVal vPass As String, ByVal vIPAddress As String, ByVal vCompanyID As String) As STC_SMSReceive()
        Try
            'Dim vUser As String = "RahyabSMSService" 'mdlGeneral.SmsSendingUsername
            'Dim vPass As String = "smsonline123@" 'mdlGeneral.SmsSendingPassword
            'Dim vNumber As String = "10002" 'mdlGeneral.SmsSendingNumber
            'Dim vIPAddress As String = "http://80.253.153.130:2055/CPSMSService/Access" 'mdlGeneral.SmsSendingIPAddress


            Dim StrXML As String


            ''''''''''''''''''''' Read All SMS Inbox 
            StrXML = "<?xml version=""1.0""?>" & vbCrLf
            StrXML = StrXML & "<!DOCTYPE smsPoll PUBLIC """" ""http://www.pervasive.ir/dtd/Cpas.dtd"" []>" & vbCrLf
            StrXML = StrXML & "<smsPoll company=""" & vCompanyID & """/> "
            ''''''''''''''''''''''''''''''''''''''''

            ''''''''''''''''''''' Read Single SMS Status
            'StrXML = "<?xml version=""1.0""?>" & vbCrLf
            'StrXML = StrXML & "<!DOCTYPE smsStatusPoll PUBLIC ""-//PERVASIVE//DTD CPAS 1.0//EN"" ""http://www.pervasive.ir/dtd/Cpas.dtd"" []>" & vbCrLf
            'StrXML = StrXML & "<smsStatusPoll company=""RAHYAB"" >" & vbCrLf
            'StrXML = StrXML & "<batch batchID=""RAHYAB+20090523044209533"" />" & vbCrLf
            'StrXML = StrXML & "</smsStatusPoll>"
            '''''' Status WE read : CHECK_OK , SMS_SENT , SMS_ERROR , SMS_FAILED
            ''''''''''''''''''''''''''''''''''''''''''''''' 

            ' Here's the data I'll be sending.  It's the same format
            ' you'd use in a querysting and you should URLEncode any
            ' data that may need it.
            Dim vDataToPost As String = StrXML
            Dim vWebRequest As WebRequest
            Dim vRequestStream As Stream

            Dim vStreamWriter As StreamWriter

            Dim vWebResponse As WebResponse
            Dim vResponseStream As Stream
            Dim vStreamReader As StreamReader

            ' Create a new WebRequest which targets that page with
            ' which we want to interact.
            vWebRequest = WebRequest.Create(vIPAddress)
            'vWebRequest = WebRequest.Create("http://87.107.12.14:2040/rpgSentItems/getSentItems")


            ' Set the method to "POST" and the content type so the
            ' server knows to expect form data in the body of the
            ' request.

            vWebRequest.Method = "POST"
            vWebRequest.ContentType = "text/xml" '"application/x-www-form-urlencoded"
            '.Headers.Add(HttpRequestHeader.Authorization, "Basic " & Base64Encode(vUser & ":" & vPass))

            'vWebRequest.Timeout = 99000000

            vWebRequest.Headers.Add("authorization", "Basic " & Base64Encode(vUser & ":" & vPass))

            ' Get a handle on the Stream of data we're sending to
            ' the remote server, connect a StreamWriter to it, and
            ' write our data to the Stream using the StreamWriter.
            vRequestStream = vWebRequest.GetRequestStream()
            vStreamWriter = New StreamWriter(vRequestStream)
            vStreamWriter.Write(vDataToPost)
            vStreamWriter.Flush()
            vStreamWriter.Close()
            vRequestStream.Close()

            ' Get the response from the remote server.
            vWebResponse = vWebRequest.GetResponse()

            ' Get the server's response status?

            ' Just like when we sent the data, we'll get a reference
            ' to the response Stream, connect a StreamReader to the
            ' Stream and use the reader to actually read the reply.
            vResponseStream = vWebResponse.GetResponseStream()
            vStreamReader = New StreamReader(vResponseStream)
            StrXML = vStreamReader.ReadToEnd()
            vStreamReader.Close()
            vResponseStream.Close()

            ' Close the WebResponse
            vWebResponse.Close()


            Dim ParseXML As String
            ParseXML = StrXML.Substring(0, 43) + StrXML.Substring(146)

            Dim XmlReader As New Xml.XmlDocument

            XmlReader.XmlResolver = Nothing
            XmlReader.InnerXml = StrXML

            Dim stc_Res As STC_SMSReceive() = Nothing


            Dim ndRoot = XmlReader.DocumentElement

            'Dim strBatchID As String
            'If ndRoot.HasChildNodes And Not ndRoot.ChildNodes(0) Is Nothing Then
            '    strBatchID = ndRoot.ChildNodes(0).Attributes(0).Value
            'End If

            Dim i As Integer = 0
            For Each ndBatch As System.Xml.XmlNode In ndRoot.ChildNodes

                For Each ndSMS As System.Xml.XmlNode In ndBatch.ChildNodes
                    '

                    If stc_Res Is Nothing Then
                        ReDim Preserve stc_Res(0)
                    Else
                        ReDim Preserve stc_Res(stc_Res.GetLength(0))
                    End If

                    stc_Res(stc_Res.GetLength(0) - 1).SourceNumber = ndSMS.ChildNodes(1).InnerText


                    stc_Res(stc_Res.GetLength(0) - 1).SenderNumber = ndSMS.ChildNodes(0).InnerText
                    'stc_Res(stc_Res.GetLength(0) - 1).Message = DecodeUCS2(ndSMS.ChildNodes(1).InnerText)
                    stc_Res(stc_Res.GetLength(0) - 1).Message = If(ndSMS.ChildNodes(4).InnerText = "t", DecodeUCS2(ndSMS.ChildNodes(2).InnerText), ndSMS.ChildNodes(2).InnerText)

                    stc_Res(stc_Res.GetLength(0) - 1).ReceiveDate = CDate(ndSMS.ChildNodes(3).InnerText)

                    stc_Res(stc_Res.GetLength(0) - 1).RecieveBatchID = ndRoot.ChildNodes(i).Attributes(0).Value 'strBatchID
                Next

                i = i + 1
            Next



            Return stc_Res


        Catch ex As Exception


            Throw ex

        End Try
    End Function

    Private Function Base64Encode(ByVal inData As String) As String
        'rfc1521
        '2001 Antonin Foller, Motobit Software, http://Motobit.cz
        Const Base64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        Dim sOut As String = ""
        Dim I As Integer

        'For each group of 3 bytes
        For I = 1 To Len(inData) Step 3
            Dim nGroup, pOut As String

            'Create one long from this 3 bytes.
            nGroup = &H10000 * Asc(Mid(inData, I, 1)) + _
              &H100 * MyASC(Mid(inData, I + 1, 1)) + MyASC(Mid(inData, I + 2, 1))

            'Oct splits the long To 8 groups with 3 bits
            nGroup = Oct(nGroup)

            'Add leading zeros
            nGroup = StrDup(8 - Len(nGroup), "0") & nGroup

            'Convert To base64
            pOut = Mid(Base64, CLng("&o" & Mid(nGroup, 1, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 3, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 5, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 7, 2)) + 1, 1)


            'Add the part To OutPut string
            sOut = sOut + pOut

            'Add a new line For Each 76 chars In dest (76*3/4 = 57)
            'If (I + 2) Mod 57 = 0 Then sOut = sOut + vbCrLf
        Next
        Select Case Len(inData) Mod 3
            Case 1 '8 bit final
                'sOut = Left(sOut, Len(sOut) - 2) + "=="
                sOut = sOut.Substring(0, Len(sOut) - 2) + "=="



            Case 2 '16 bit final
                'sOut = Left(sOut, Len(sOut) - 1) + "="
                sOut = sOut.Substring(0, Len(sOut) - 1) + "="
        End Select
        Base64Encode = sOut
    End Function

    Private Function MyASC(ByVal OneChar As String) As Integer
        If OneChar = "" Then MyASC = 0 Else MyASC = Asc(OneChar)
    End Function

    Public Function ReadXML(ByVal txt As String) As Array
        Try



            Dim StrRecivedMessage(100000000) As String
            Dim I As Integer



            'Dim settings As XmlReaderSettings = New XmlReaderSettings()
            'settings.ProhibitDtd = True
            Dim reader As System.Xml.XmlReader = System.Xml.XmlReader.Create(New System.IO.StringReader(txt)) ', settings)



            Dim Element As String = ""
            Dim StrSMS As String = ""
            Dim StrMessage As String = ""
            Dim strTmp = ""
            I = 0
            Do While (reader.Read())
                Select Case reader.NodeType
                    Case XmlNodeType.Element 'Display beginning of element.

                        'MessageBox.Show("<" + reader.Name)
                        Element = reader.Name
                        If reader.HasAttributes Then 'If attributes exist
                            While reader.MoveToNextAttribute()
                                'Display attribute name and value.
                                If Element = "batch" Then

                                    StrSMS = StrSMS + reader.Name + " = " + reader.Value
                                    I = I + 1
                                    StrRecivedMessage(I) = reader.Value
                                    'MessageBox.Show(reader.Name + " = " + reader.Value)
                                End If
                            End While
                        End If
                        ' MessageBox.Show(">")
                    Case XmlNodeType.Text 'Display the text in each element.
                        If Element = "time" Then
                            'StrSMS = StrSMS + " Time :" + reader.Value
                            I = I + 1
                            StrRecivedMessage(I) = reader.Value
                        End If

                        If Element = "binary" Then
                            If reader.Value = "t" Then
                                strTmp = DecodeUCS2(strTmp)
                            ElseIf reader.Value = "f" Then
                                strTmp = strTmp

                            End If
                            I = I + 1
                            StrRecivedMessage(I) = strTmp
                        End If


                    Case XmlNodeType.CDATA  'Display end of element.
                        If Element = "origAddr" Then
                            'StrSMS = StrSMS + " origAddr :" + reader.Value
                            I = I + 1
                            StrRecivedMessage(I) = reader.Value

                        ElseIf Element = "message" Then
                            'StrMessage = DecodeUCS2(reader.Value)
                            strTmp = reader.Value

                            ' StrSMS = StrSMS + " message  :" + StrMessage
                            ' I = I + 1
                            'StrRecivedMessage(I) = reader.Value

                            'MessageBox.Show("<" + reader.Value + ">")
                        End If
                End Select
            Loop
            '   MessageBox.Show(StrSMS.ToString())
            Return StrRecivedMessage
        Catch ex As Exception
            Throw
        End Try

    End Function

    Public Function DecodeUCS2(ByVal Content As String) As String
        On Error GoTo errHandler

        Dim hextext As String = Content
        Dim i As Long, ret As String
        ret = ""
        For i = 1 To Len(hextext) Step 4
            ret = ret & ChrW(Val("&h" & Mid(hextext, i, 4)))
        Next
        Return ret

        Exit Function
errHandler:
        Return hextext
    End Function

End Class


