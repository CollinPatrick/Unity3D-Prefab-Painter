# Unity3D-Prefab-Painter

### A tool for quickly creating, placing, and initializing prefabs in a scene 

![image](https://user-images.githubusercontent.com/40306723/219966379-9972dd5b-fefd-4056-856c-642d7fda1f15.png)

## Setup
1. Find the PrefabPainter.cs script inside the Editor folder (Prefab Painter/Editor) 
   - ![image](https://user-images.githubusercontent.com/40306723/219966927-d95d87a3-7ebe-4fe9-ad75-da97ac5a4a18.png)

2. locate the EDITOR_RESOURCE_DIRECTORY constant variable inside the script and set the directory path to the path of the root "Prefab Painter" folder.
   - ![image](https://user-images.githubusercontent.com/40306723/219966876-aa8668ee-7ac6-4196-846e-ba8370a920e8.png)

3. Add the "Prefab Painter Object" prefab to your scene.
   - ![image](https://user-images.githubusercontent.com/40306723/219966852-ee16a07a-3883-436e-a04b-b5f03755f334.png)

4. Either click the "Open Prefab Painter" button, or manually open the prefab painter window via Tools/Prefab Painter in the top menu bar.
   - ![image](https://user-images.githubusercontent.com/40306723/219966782-f9637b6f-c826-487d-8219-c9be337007b8.png)
   
## How To use

#### Prefab Groups
![image](https://user-images.githubusercontent.com/40306723/219969418-8ceb7419-bba1-47bc-a682-8aaacea9b2cb.png)

Prefab groups contain all the paintable prefabs neatly organized into groups. To add or remove a group, simply type in the name of the group and click the add or remove button. To add a prefab to a group, drag and drop the prefab from the project window to the square with a *+* icon inside the group you want to add to. To remove a prefab, press the *-* button at the top right of the prefab icon. To paint, simply click on whatever prefab you want to paint from the Prefab Gropus section and make sure the "Prefab Painter Object" is selected in the hierarchy.

#### Brush
![image](https://user-images.githubusercontent.com/40306723/219969081-d50f2114-82c3-4c34-b302-ec0fc6a30924.png)

- Brush Size
  - How large of an area to paint prefabs.

- Brush Weight
  -  How many objects to spawn relative to the area of the brush.
  -  Brush Area * Brush Weight

- Max Objects
  - The max number of objects that can be instanced per paint.
  - This is intended to help performance when using larget brush sizes.

- Ignore layers
  - The painter will ignore these layers when finding paintable surfaces

- Painter Tag
  - If not set to Untagged, any instanced prefabs will have their tag set to this tag if their default tag is also untagged.

#### Rotation
![image](https://user-images.githubusercontent.com/40306723/219969070-47d13c9f-d813-47d6-88c6-f71221feeca9.png)

- Randomize Rotation
  - Enable to randomize the rotation of a painted prefab.

- X Random Rotation
  - The random range to rotate the painted object on the X axis.
  
- Y Random Rotation
  - The random range to rotate the painted object Y axis.
  
- Z Random Rotation
  - The random range to rotate the painted object Z axis.

#### Scale
![image](https://user-images.githubusercontent.com/40306723/219969047-91645f6a-a6f7-4be2-9389-58a1b68fca59.png)

- Randomize Scale
  - Enable to randomize the scale of a painted prefab.
  
- Uniform Scale
  - Should the object be scaled the same on all axis?
  
- Uniform Random Scale
  - The random range to scale the painted object on all axis.

- X Random Scale
  - The random range to scale the painted object on the X axis.
  
- Y Random Scale
  - The random range to scale the painted object Y axis.
  
- Z Random Scale
  - The random range to scale the painted object Z axis.

#### Prefab Painter Controller
![image](https://user-images.githubusercontent.com/40306723/219969022-c75232d4-c395-4454-80d7-1ffd10171b2a.png)

- Parent Object
  - The Transform that all painted objects will be parented to.
  
- Processors
  - The list of processors to apply to all painted objects.

## Prefab Painter Processors
The prefab painter allows you to extend functonality for custom object setup and placement. To create a new processor, inherit from the PrefabPainterProcessor base class and override the virtual methods it contains.

Determines what component (on its root) a prefab must contain for the processor to be used. 
```
public virtual Type typeFilter => typeof( Transform );
```

Runs when a prefab is instanced. Passes the prefab and if this is before or after the internal instancing setup.  
```
public virtual void OnIntance( ProcessStage aStage, GameObject aGameObject ) { }
```

Runs when a prefab is positioned. Passes the prefab and if this is before or after the internal positioning.  
```
public virtual void OnPosition( ProcessStage aStage, GameObject aGameObject ) { }
```

Runs when a prefab is Rotated. Passes the prefab and if this is before or after the internal rotation.  
```
public virtual void OnRotation( ProcessStage aStage, GameObject aGameObject ) { }
```

Runs when a prefab is scaled. Passes the prefab and if this is before or after the internal scaling.  
```
public virtual void OnScale( ProcessStage aStage, GameObject aGameObject ) { }
```

### Using A Cusrom Processor
To use a custom processor, attach it to the "Prefab Painter Object" in the scene then add it to the list of processors in the "Prefab Painter Controller" component.
To stop using a processor, you can either remove it from the list or processors or optionally uncheck the "Is Enabled" flag on the processor component.
