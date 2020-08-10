This project is just for the demonstration of Serializing and Deserializing  of RichTextBox using `XamlReader` and `XamlWriter` when RichTextBox contains Bitmap(Pasted from clipboard)

This project is the of the Question [here](https://stackoverflow.com/questions/63165819/problem-when-serializing-and-de-serializing-richtextbox-in-wpf)



# Main Window in this DEMO

![image-20200809223058727](D:\dev\dev_dotnet\workspace\richtextbox\richtextbox\images\image-20200809223058727.png)



#  The left panel is a TreeView element:

```C#
<TreeView ItemsSource="{Binding Categories}">
    <TreeView.ItemContainerStyle>
        <Style TargetType="{x:Type TreeViewItem}">
            <Setter Property="ForceCursor" Value="True"/>
            <Style.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="FontWeight" Value="Bold" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </TreeView.ItemContainerStyle>
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding SubCategories}">
            <Grid>
                <TextBlock Text="{Binding Title}">                                           
                </TextBlock>                        
            </Grid>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="SelectedItemChanged">
            <mvvm:EventToCommand Command="{Binding Event_TreeSelectionChangedCommand, Mode=OneWay}" 
                                            PassEventArgsToCommand="True"></mvvm:EventToCommand>
        </i:EventTrigger>
    </i:Interaction.Triggers>
</TreeView>
```

# Node data is saved in DB:

![image-20200809221700226](D:\dev\dev_dotnet\workspace\richtextbox\richtextbox\images\image-20200809221700226.png)

# Right side

The Right side panel is a UserControl `RichTextBoxCtrl`.  This Control looks pretty simple:

```xaml
<UserControl x:Class="richtextbox.userctrls.RichTextBoxCtrl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" >
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <WrapPanel Orientation="Horizontal">
            <ToolBarTray>
                
            </ToolBarTray>
        </WrapPanel>
        <RichTextBox Grid.Row="1"
            x:Name="ContentHolder" 
            AcceptsTab="True" AcceptsReturn="True" 
            VerticalScrollBarVisibility="Visible"
            HorizontalScrollBarVisibility="Auto">            
        </RichTextBox>
    </Grid>
</UserControl>
```

Its main element is a `RichTextBox`

The Database is just contains only on table.

![image-20200809222202511](D:\dev\dev_dotnet\workspace\richtextbox\richtextbox\images\image-20200809222202511.png)

I have filled necessary data for testing.

I've tried 3 methods to serialize and deserialize FlowDocument of RichTextBox. The last two use same serializing/deserializing method





# XAML_WRITER_LOADER





**Serializing Method:**

```C#
private byte[] SerializeXamlReaderWriter(FlowDocument doc) {
    MemoryStream ms = new MemoryStream();
    XamlWriter.Save(doc, ms);
    byte[] data = ms.ToArray();
    return data;
}
```



**Deserializing Method:**

```C#
private FlowDocument DeserializeXamlReaderWriter(byte[] data) {
    MemoryStream ms = new MemoryStream(data);
    FlowDocument rtbLoad = (FlowDocument)XamlReader.Load(ms);
    return rtbLoad;
}
```



If we use a any Screen Capture Tool to capture a screenshot, and copy this screenshot (`Ctrl + C`) to clipboard and Paste(`Ctrl + V`) it to the `RichTextBox`. Then save this Document by press `Ctrl +S`.  **Then Close the program. Re-open the program again.**  Then on the Left panel. client the relative Node(It should be the third or fourth). The following exception will be thrown:

```C#
System.Windows.Markup.XamlParseException
  HResult=0x80131501
  Message='Initialization of 'System.Windows.Media.Imaging.BitmapImage' threw an exception.' Line number '1' and line position '360'.
  Source=PresentationFramework
  StackTrace:
   at System.Windows.Markup.WpfXamlLoader.Load(XamlReader xamlReader, IXamlObjectWriterFactory writerFactory, Boolean skipJournaledProperties, Object rootObject, XamlObjectWriterSettings settings, Uri baseUri)

  This exception was originally thrown at this call stack:
    System.Net.WebRequest.Create(System.Uri, bool)
    System.Net.WebRequest.Create(System.Uri)
    MS.Internal.WpfWebRequestHelper.CreateRequest(System.Uri)
    System.IO.Packaging.PackWebRequest.GetRequest(bool)
    System.IO.Packaging.PackWebRequest.GetResponse()
    MS.Internal.WpfWebRequestHelper.GetResponse(System.Net.WebRequest)
    System.Windows.Media.Imaging.BitmapDecoder.SetupDecoderFromUriOrStream(System.Uri, System.IO.Stream, System.Windows.Media.Imaging.BitmapCacheOption, out System.Guid, out bool, out System.IO.Stream, out System.IO.UnmanagedMemoryStream, out Microsoft.Win32.SafeHandles.SafeFileHandle)
    System.Windows.Media.Imaging.BitmapDecoder.CreateFromUriOrStream(System.Uri, System.Uri, System.IO.Stream, System.Windows.Media.Imaging.BitmapCreateOptions, System.Windows.Media.Imaging.BitmapCacheOption, System.Net.Cache.RequestCachePolicy, bool)
    System.Windows.Media.Imaging.BitmapImage.FinalizeCreation()
    System.Windows.Media.Imaging.BitmapImage.EndInit()
    ...
    [Call Stack Truncated]

Inner Exception 1:
NotSupportedException: The URI prefix is not recognized.
```



