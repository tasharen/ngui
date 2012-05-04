----------------------------------------------
            NGUI: Next-Gen UI kit
 Copyright © 2011-2012 Tasharen Entertainment
                Version 2.0.4c
    http://www.tasharen.com/?page_id=197
            support@tasharen.com
----------------------------------------------

Thank you for buying NGUI!

If you have any questions, suggestions, comments or feature requests, please
drop by the NGUI forum, found here: http://www.tasharen.com/forum/index.php?board=1.0

--------------------
 How To Update NGUI
--------------------

If you have the Professional Edition of NGUI that comes with Git access, just pull the latest changes.

If you have a Standard Edition:

1. In Unity, File -> New Scene
2. Delete the NGUI folder from the Project View.
3. Import NGUI from the updated Unity Package.

---------------------------------------
 Support, documentation, and tutorials
---------------------------------------

All can be found here: http://www.tasharen.com/?page_id=197

Using NGUI with JavaScript (UnityScript)? Read this first: http://www.tasharen.com/forum/index.php?topic=6

-----------------
 Version History
-----------------

2.0.4:
- NEW: Added UIButton -- same as UIButtonColor, but has a disabled state.
- NEW: Added the OnDoubleClick event. Same as OnClick, just sent on double-click.
- FIX: UIDraggablePanel should now have noticeably better performance with many widgets.
- FIX: All private serializable properties will now be hidden from the inspector.
- FIX: UITooltip is now more robust and automatically uses background border size for padding.
- FIX: UILabel inspector now uses a word-wrapped textbox.
- FIX: UIButtonPlayAnimation and UIButtonTween now have an event receiver (on finished).
- FIX: UIGrid no longer modifies Z of its items on reposition.
- FIX: Only one Localization class is now allowed to be present.
- FIX: UILabel should now have a bit better performance in the editor.
- FIX: UISprite's MakePixelPerfect setting now takes padding into account properly.

2.0.3:
- NEW: UIButtonSound now allows you to specify pitch in addition to volume.
- FIX: UIDraggablePanel will now update the scroll bars on start.
- FIX: UITweenScale will now start with a scale of one instead of zero by default.
- FIX: UIInput will now ignore all characters lower than space, fixing an issue with mac OS input.
- FIX: UITexture will no longer lose its material whenever something changes.
- FIX: Reworked the way the mouse is handled in UICamera, fixing a couple of highlighting issues.

2.0.2:
- FIX: UIButton series of scripts will now correctly disable and re-enable their selected state when the game object is enabled / disabled.
- FIX: SpringPanel will now notify the Draggable Panel script on movement, letting it update scroll bars correctly.
- FIX: UIDraggablePanel will now lose its momentum every frame rather than only when it's being dragged.
- FIX: UIDraggablePanel will no longer reset the panel's position on start.
- FIX: UIDraggablePanel.ResetPosition() now functions correctly.
- FIX: UIDraggablePanel.UpdateScrollbars() will now only adjust the position if the scroll bars aren't being updated (ie: called from a scroll bar).
- FIX: 3D UIs will now be created with a proper anchor offset.

2.0.1:
- NEW: UIDraggablePanel will now display the bounds of the draggable widgets as an orange outline in the Scene View.
- NEW: Added a 'repositionNow' checkbox to UIDraggablePanel that will reset the clipping area using the children widget's current bounds.
- NEW: It's now possible to specify horizontal and vertical axis names for UICamera.
- FIX: UICamera will no longer process WASD or Space key events if an Input Field is currently selected.
- FIX: UIDraggablePanel's 'startingDragAmount' was renamed to 'startingRelativePosition', for clarity.
- FIX: UICheckbox will now set the checkmark state immediately on start instead of gradually.
- FIX: UISlider will now always force-set its value value on start.
- FIX: UIInput.text will now always return its own text rather than that of the label (works better with captions).
- FIX: Setting UIInput.text now sets the color of the label to the active color.

2.0.0:
- NEW: Redesigned the way UIDragCamera and UIDragPanelContents work, making them much more straightforward.
- NEW: New widget has been added: Scroll Bar. It does exactly what you think it does.
- NEW: UIDraggableCamera script is used on the camera to make it draggable via UIDragCamera.
- NEW: UIDraggablePanel script is used on the panel to make it draggable via UIDragPanelContents.
- NEW: UIDraggablePanel natively supports scroll bars with "always show", "fade out if not needed" and "fade in only when dragging" behaviors.
- NEW: Scroll View (DragPanel) and Quest Log examples have been updated with scroll bars.
- NEW: Reorganized all examples to be in a more logical order -- starting with the basic, common functionality and going up from there.
- NEW: Localization will now try to automatically load the language file via Resources.Load if it wasn't found in the local list.
- NEW: Atlas Maker tool now allows you to turn off trimming of transparent pixels before importing certain sprites.
- NEW: Atlas Maker tool now allows you to specify how much padding is applied in-between of sprites.
- FIX: EditorPrefs are now used instead of PlayerPrefs to store editor-related data.
- FIX: Popup list will no longer try to call SendMessage in edit mode.
- FIX: UIEventListener.Add is now UIEventListener.Get, making the function make more sense with the -= operator.
- DEL: Scroll View example that was using UIDragObject has been removed as it's now obsolete.

1.92:
- NEW: Expanded the Filled Sprite to support radial-based filling. Great for progress indicators, cooldown timers, circular health bars, etc.
- FIX: Eliminated all runtime uses of 'foreach', seeing as it causes memory leaks on iOS.

1.91:
- NEW: Added a new example scene showing how to easily implement drag & drop from 2D UI to the 3D world.
- FIX: UICamera was sending multiple OnDrag events for the mouse. This has now been fixed.
- FIX: UIAnchor changes in 1.90 had a few adverse effects on two of the examples.

1.90:
- NEW: You can now specify an option on the UIDragPanelContents that will prevent dragging if the contents already fit.
- NEW: You can now specify a radio button group root on the checkbox instead of always having it be the parent object.
- NEW: You can now easily adjust the widget's alpha by using the new UIWidget.alpha property.
- NEW: UIAnchor script has been redesigned, and the 'stretch to fill' property has been removed. You can now position using relative coordinates.
- NEW: UIStretch script has been added, allowing you to stretch an object in either (or both) directions using relative coordinates.
- NEW: You can now specify a maximum range distance for UICamera's raycasts, allowing you to limit the interaction distance (for first-person cameras).
- FIX: Popup list inspector now shows the "Position" drop-down.
- FIX: Slider now updates correctly when it's first created, and when you change the Full Size property.
- FIX: UIDragCamera now takes the camera's size into consideration.
- FIX: DestroyImmediate calls have been replaced with NGUITools.DestroyImmediate as there seem to be odd issues on certain Android devices.

1.88:
- NEW: Added an option to the tweener to use steeper pow(2) curves for ease in/out tweens.
- NEW: You can now specify the movement threshold that will be used to determine whether button presses are eligible for clicks on UICamera.
- NEW: You can now specify an input field to be password-based, and it will only hide the text once you start typing.
- FIX: UIButtonTween can now disable objects properly after a toggle.
- FIX: UISavedOption can now save the state of a single checkbox in addition to a group of checkboxes.
- FIX: Localization now handles duplicate key entries silently.
- FIX: Widgets not using a texture will now have gizmos.
- FIX: Fix for the OnClick event on touch-based devices.

1.87:
- NEW: UISlider now has an inspector class, and 'rawValue' can no longer be modified (use 'sliderValue'!)
- FIX: An assortment of tweaks and fixes, focusing on stability and ease of use.
- FIX: Reworked the way the UIPopupList was calculating its padding, making it more robust.
- FIX: Disabled widgets will get updated correctly when the atlas gets replaced.
- FIX: Disabling the button on click should no longer make it get stuck in the "clicked" state.
- FIX: UICamera.lastTouchPosition is back.

1.86:
- NEW: UIAtlas now has a "pixel size" property that affects MakePixelPerfect logic as well as sliced sprite's border size.
- FIX: UISprite will now always ensure it has a sprite to work with, if at all possible.
- FIX: UIDragPanelContents should now work correctly on mobile devices.

1.85:
- NEW: Added Example 12: Better Scroll View.
- NEW: Added a script that can be used to efficiently drag the contents of the panel: UIDragPanelContents.
- NEW: Added a function replacement for SetActiveRecursively (NGUITools.SetActive), since the former has rare odd issues.

1.84:
- FIX: Changed the way the font data is stored, resulting in potentially better loading performance on mobile devices.
- FIX: UIPanel.Start() should now find cameras faster.
- FIX: UIPanel will no longer use the clipping softness value unless soft clipping is actually used.
- FIX: The way click / drag was handled has been changed a bit. It should now be easier to click buttons on retina screens.
- FIX: Rebuilding an atlas was not updating fonts correctly.
- FIX: Couple of tweaks to UIAtlas and UIFont's replacement feature.

1.83:
- NEW: Added a simple script that can save the state of the checkbox (or a group of checkboxes) to player prefs.
- FIX: A variety of minor tweaks.

1.82:
- NEW: It's now possible to specify a "replacement" value on UIAtlas and UIFonts, making swapping of atlases and fonts a trivial matter.
- NEW: UICheckbox now has an option to allow unchecking the last item within a group.
- FIX: Most cases of FindObjectsOfTypeAll has been replaced with FindSceneObjectsOfType instead.
- FIX: UISliderColors now keeps the slider's alpha.
- FIX: Edit-time modification of UISlider's 'rawValue' in the inspector will now again visibly move the slider.
- FIX: UIWidget will no longer consider its geometry as changed every frame if there is nothing to draw (empty text labels).
- FIX: Atlas Maker will now create a new font if the name of the font doesn't match.
- OLD: NGUITools.ReplaceAtlas and Font functions have been deprecated.

1.81:
- NEW: UIInput can now be multi-line.
- FIX: UILabel will now center-align properly again when a fixed line width was specified.
- FIX: UILabel's effect (shadow, outline) will now be affected by the label's alpha.
- FIX: UILabel's effect will now always be offset consistently, even if the scale changes.
- FIX: Changing the widget's pivot will no longer cause it to become it pixel-perfect.
- FIX: UISlider no longer requires a box collider.
- FIX: Creating sliders via the wizard will now set their full size property.

1.80:
- NEW: You can now add a colored shadow/bevel or an outline effect to your labels via a simple checkbox.
- NEW: UICamera now has support for keyboard, joystick and controller input.
- NEW: UICamera can now control what kind of events it will process (only touch, only keyboard, etc).
- NEW: UISlider can now be adjusted via keyboard, joystick and controller input.
- NEW: UIPopupList can now be interacted with using a keyboard or controller input.
- NEW: Added a new script, UIButtonKeys that can be used to set up the UI for keyboard, joystick and controller input.
- NEW: New Example 11 shows how to set up the UI to work with the new input types.

1.70:
- NEW: Right click stuff has been replaced by just 'lastTouchID' with added support for the middle mouse button.
- NEW: UIDragCamera now has scrolling wheel support just like UIDragObject.
- FIX: UTF8 encoding is not supported in Flash. Wrote my own binary parsing function to make Flash work.
- FIX: UILabel will now align to right and center properly when not pixel-perfect.
- FIX: UIFont.WrapText will now trim away space characters when word wrapping.
- FIX: UIFont.Print will no longer draw spaces (padding will still be applied).
- FIX: UIPopupList will highlight the correct item even when localized.
- FIX: UITable will now handle disabled children properly.
- FIX: Fixed a crash on Unity 3.5.0 (sigh!).
- FIX: Tweaked how pixel-perfect calculations work for labels.

1.69:
- NEW: Added right-click support by simply adding an optional integer parameter to the OnClick event.
- NEW: The contents of the UIPopupList can now be localized by enabling a checkbox on it.
- NEW: You can now give the UIEventListener an optional parameter that you can retrieve later.

1.68:
- NEW: Added a built-in Localization System.
- NEW: Added a new example (10) - Localization.
- FIX: Widgets can now be modified directly on prefabs.
- FIX: Fixed the window stuttering in example 9 (when dragging it).
- FIX: Widgets will now ensure they're under the right panel after drag & drop in the editor.
- FIX: It's now possible to visibly modify the value of the slider at edit mode.
- FIX: Scaling labels now properly rebuilds them.
- FIX: Scaling labels will no longer affect the widget bounds in odd ways.

1.67:
- FIX: Font Maker's Replace button will now re-import the data file.
- FIX: Fixed all known issues with Undo functionality.
- FIX: Fixed all known issues with prefabs (mainly 3.5.0-related)
- FIX: Fixed clipping in Flash by adding a work-around for a bug in Flash export.
- FIX: Removed 3.5b6 work-arounds for Flash as the bug has since been fixed.

1.66:
- NEW: Added a new script: ShaderQuality. It's used to automatically set shader level of detail as the quality level goes down.
- FIX: All examples have been updated to run properly in Flash.
- FIX: NGUI now has no warnings using Unity 3.5.0.

1.65:
- NEW: Example 9: Quest Log shows how to make a fancy quest log.
- NEW: Added a new feature to UIPanel -- the ability to write to depth before any geometry is drawn. This doubles the draw calls but saves fillrate.
- NEW: Clicking on the items in the panel and camera tools will now select them instead of enable/disable them.
- NEW: UITable can now automatically keep its contents within the parent panel's bounds.
- NEW: New event type: OnScroll(float delta).
- FIX: FindInChildren was not named properly. It's now FindInParents.
- FIX: Eliminated most warnings on Unity 3.5.

1.64:
- NEW: Atlas inspector window now shows "Dimensions" and "Border" instead of "Outer" and "Inner" rects.
- NEW: UIPanel now has an optional property: "showInPanelTool" that determines whether the panel will show up in the Panel Tool.
- FIX: Trimmed sprite-using fonts will now correctly trim the glyphs.
- FIX: The "inner rect" outline now uses a checker texture, making it visible regardless of sprite's color.
- FIX: Selected sprite within the UIAtlas is now persistent.
- FIX: Panel and Camera tools have been improved with additional functionality.

1.63:
- NEW: Added a logo to all examples with some additional shiny functionality (contributed by Hjupter Cerrud).
- NEW: Label template in the Widget Tool now has a default color that will be applied to newly created labels.
- NEW: Added an option to TweenScale to automatically notify the UITable of the change.
- FIX: Updating a texture atlas saved as a non-PNG image will now update the texture correctly.
- FIX: Updating an atlas with a font sprite in it will now correctly mark all fonts using it as dirty.
- FIX: Fixed all remaining known issues with the Atlas Maker.
- FIX: Tiled Sprite will now use an inner rect rather than outer rect, letting you add some padding.
- FIX: UIButtonTween components will now set their target in Awake() rather than Start(), fixing a rare order-of-execution issue.
- FIX: UITable will now consider the item's own local scale when calculating bounds.
- DEL: "Deprecated" folder has been deleted.

1.62:
- NEW: Added a new class -- UITable -- that can be used to organize its children into rows/columns of variable size (think HTML table).
- FIX: Font Maker will make it more obvious when you are going to overwrite a font.
- FIX: Tweener will now set its timestamp on Start(), making tweens that start playing on Play behave correctly.
- FIX: UISlicedSprite will now notice that its scale is changing and will rebuild its geometry properly.
- FIX: Atlas and Font maker will now create new atlases and fonts in the same folder as the selected items.

1.61:
- NEW: UICheckbox.current will hold the checkbox that triggered the 'functionName' function on the 'eventReceiver'.
- FIX: UIPopupList will now place the created object onto a proper layer.

1.60:
- NEW: Added a built-in atlas-making solution: Atlas Maker, making it possible to create atlases without leaving Unity.
- NEW: Added a tool that makes creation of fonts easier: Font Maker. Works well with the Atlas Maker.
- FIX: UIAtlasInspector will now always force the atlas texture to be of proper size whenever the material or texture packer import gets triggered.
- FIX: Removed the work-around for Flash that disabled sound, seeing the bug has been since fixed.
- FIX: Tweener has been renamed to NTweener to avoid name conflicts with HOTween.
- FIX: An assortment of minor usability tweaks.

1.50:
- NEW: The UI is now timeScale-independent, letting you pause the game via Time.timeScale = 0.
- NEW: Added an UpdateManager class that can be used to programmatically control the order of script updates.
- NEW: NGUITools.PlaySound() now returns an AudioSource, letting you change the pitch.
- FIX: UIAtlas and UIFont now work with Textures instead of Texture2Ds, letting you use render textures.
- FIX: Typewriter effect script will now pre-wrap text before printing it.
- FIX: NGUIEditorTools.SelectedRoot() no longer considers prefabs to be valid.
- FIX: TexturePacker import will automatically strip out the ".png" extension from script names.
- FIX: Tested and working with the Flash export as of 3.5.0 f3.

1.49:
- NEW: UIWidgets now work with Textures rather than Texture2D, making it possible to use render textures if desired.
- FIX: Rewrote the UIFont's WrapText function. It now supports wrapping of long lines properly.
- FIX: Input fields are now multi-line, and will now show the last line when typing past the label's width.
- FIX: Input fields will now update less frequently when IME or iOS/Android keyboard is used.

1.48:
- NEW: Added a new container class -- BetterList<>. It replaced the generic List<> in many cases, eliminating GC spikes.
- FIX: Various performance-related optimizations.
- FIX: UITextList will now handle resized text labels correctly.
- FIX: Parenting and reparenting widgets will now cause their panel to get updated correctly.
- FIX: Eliminated one potential cause of widgets trying to update before being parented.

1.47:
- NEW: Added a new example (8) showing how to create a simple menu system.
- NEW: Added an example script that adds a typewriter effect to labels.
- NEW: Added a 'text scale' property to the UIPopupList.
- FIX: UIPopupList will now choose a more appropriate depth rather than just a high number.
- FIX: UIPopupList labels' colliders will now be properly positioned on the Z.
- FIX: Fix for UISpriteAnimationInspector not handling null strings.
- FIX: Several minor fixes for rare issues (such as playing a sound with no audio listener or main camera in the scene).

1.46:
- NEW: Added a new class (UIEventListener) that can be used to easily register event listener delegates via code without the need to create MonoBehaviours.
- NEW: Added a UIPopupList class that can be used to create drop-down lists and menus.
- NEW: Added the Popup List and Popup Menu templates to the Widget Wizard.
- NEW: UISlider can now move in increments by specifying the desired Number of Steps.
- NEW: Tutorial 11 showing how to use UIPopupLists.

1.45:
- NEW: Text labels will center or right-align their text if such pivot was used.
- NEW: Added an inspector class for the UIImageButton.
- NEW: UIGrid now has the ability to skip deactivated game objects.
- NEW: Font sprite is now imported when the font's data is imported, and will now be automatically selected from the atlas on import.
- FIX: Making widgets pixel-perfect will now make them look crisp even if their dimensions are not even (ex: 17x17 instead of 18x18).
- FIX: Component Selector will now only show actual prefabs as recommended selections. Prefab instances aren't.
- FIX: BMFontReader was not parsing lines quite right...

1.44:
- NEW: UIGrid can now automatically sort its children by name before positioning them.
- NEW: Added momentum and drag to UIDragCamera.
- NEW: Added the Image Button template to the Widget Tool.
- FIX: SpringPosition will now disable itself properly.
- FIX: UIImageButton will now make itself pixel-perfect after switching the sprites.
- FIX: UICamera will now always set the 'lastCamera' to be the camera that received the pressed event while the touch is held.
- FIX: UIDragObject will now drag tilted objects (windows) with a more expected result.

1.43:
- NEW: Added the Input template to the Widget Tool.
- NEW: UIButtonMessage will now pass the button's game object as an optional parameter.
- NEW: Tweener will now pass itself as a parameter to the callWhenFinished function.
- NEW: Tweener now has an 'eventReceiver' parameter you can set for the 'callWhenFinished' function.
- FIX: Tweener will no longer disable itself if one of OnFinished SendMessage callbacks reset it.
- FIX: Updated all tutorials to use 1.42+ functionality.
- FIX: Slider will now correctly mark its foreground widget as having changed on value change.

1.42:
- NEW: Added a new tool: Widget Creation Wizard. It replaces all "Add" functions in NGUI menu.
- NEW: Added new templates to the Widget Wizard: Button, Checkbox, Progress Bar, Slider.
- NEW: When adding widgets via the wizard, widget depth is now chosen automatically so that each new widget appears on top.
- NEW: AddWidget<> functionality is now exposed to runtime scripts (found in NGUITools).
- FIX: Widget colliders of widgets layed on top of each other are now offset by wiget's depth.
- FIX: Several minor bug fixes.

1.41:
- NEW: Added a new tool: Camera Tool. It can be used to get a bird's eye view of your cameras and determine what draws the selected object.
- NEW: Added a new tool: Create New UI. You can use it to create an entire UI hierarchy for 2D or 3D layouts with one click of a button.
- NEW: Added a new script: UIRoot. It can be used to scale the root of your UI by 2/ScreenHeight (the opposite of UIOrthoCamera).
- NEW: The NGUI menu has been enhanced. When adding widgets, it will intelligently determine where to add them best.
- NEW: Sliced sprites now have an option to not draw the center, in case you only want the border.
- FIX: Scaling sliced sprites and tiled sprites will now correctly update them again.
- FIX: Changing the depth of the widgets will now correctly update them again.
- FIX: The unnecessary color parameter specified on the material has been removed from the shaders.

1.40:
- NEW: Major performance improvements. The way the geometry was being created has been completely redone.
- NEW: With the new system, moving, rotating and scaling panels no longer causes widgets they're responsible for to be rebuilt.
- NEW: Panel clipping will now actually clip widgets, eliminating them from the draw buffers until they move back into view.
- NEW: Matrix parameter has been eliminated from the clip shaders as it's no longer needed with the new system.
- FIX: Work-around for a rare obscure issue caused by a bug in Unity related to instantiating widgets from prefabs (Case 439372).
- FIX: It's no longer possible to edit widgets directly on prefabs. Bring them into the scene first.
- FIX: Panel tool will now update itself on object selection.

1.33:
- NEW: UICheckbox now has a configurable function to call.
- NEW: UICheckbox now has an animation parameter it can trigger when checked/unchecked.
- NEW: You can now play remote animations via UIButtonPlayAnimations.
- NEW: Tweener now sends out notifications when it finishes.
- NEW: Tweener now has a 'group' parameter that can be used to only enable/disable only certain tweens on the same object.
- NEW: UIButtonTween has been changed to use more descriptive properties. Check your UIButtonTweens and update their properties after upgrading.
- NEW: Examples 1, 5 and 6 have been adjusted to use the new features.
- FIX: Scrolling speed should now be consistent even at low framerates.
- FIX: Shader fixes.

1.32:
- NEW: Added a 'thumb' parameter to the UISlider.
- NEW: Added UIForwardEvents script that can be used to forward events from one object to another.
- NEW: Added the ability to enable and disable target game objects via UIButtonTween.
- FIX: Input fields now support IME.

1.31:
- NEW: Added a panel tool (NGUI menu -> Panel Tool) to aid with multi-panel development.
- FIX: Variety of tweaks and minor enhancements, mainly related to examples.
- FIX: UIDragObject had a rare bug with how items were springing back into place.

1.30:
- NEW: UIPanels can now specify a clipping area (everything outside this area will not be visible).
- NEW: UIFilledSprite, best used for progress bars, sliders, etc (thanks nsxdavid).
- NEW: UISpriteAnimation for some simple sprite animation (attach to a sprite).
- NEW: UIAnchor can now specify depth offset to be used with perspective cameras.
- NEW: UIDragObject can now restrict dragging of objects to be within the panel's clipping bounds.
- NEW: UICheckbox now has an "option" field that lets you create option button groups (Tutorial 11).
- NEW: Example 7 showing how to use the clipping feature.
- NEW: Example 0 (Anchor) has been redone.
- NEW: Most tutorials and examples now explain what they do inside them.
- NEW: Added a variety of new interaction scripts replacing State scripts (UIButton series for example).
- NEW: Added a Drag Effect parameter to UIDragObject with options to add momentum and spring effects.
- FIX: UICamera.lastCamera was not pointing to the correct camera with multi-camera setups (thanks LKIM).
- FIX: UIAnchor now positions objects in the center of the ortho camera rather than at depth of 0.
- FIX: Various usability improvements.
- OLD: 'State' series of scripts have all been deprecated.

1.28:
- NEW: Added a simple tweener and a set of tweening scripts (position, rotation, scale, and color).
- FIX: Several fixes for rare non-critical issues.
- FIX: Flash export bug work-arounds.

1.27:
- FIX: UISlider should now work properly when centered again.
- FIX: UI should now work in Flash after LoadLevel (added some work-arounds for current bugs in the flash export).
- FIX: Sliced sprites should now look properly in Flash again (added another work-around for another bug in the Flash export).

1.26:
- NEW: Added support for trimmed sprites (and fonts) exported via TexturePacker.
- NEW: UISlider now has horizontal and vertical styles.
- NEW: Selected widgets now have their gizmos colored green.
- FIX: UISlider now uses the collider's bounds instead of the target's bounds.
- FIX: Sliced sprite will now behave better when scaled with all pivot points, not just top-left.

1.25:
- NEW: Added a UIGrid script that can be used to easily arrange icons into a grid.
- NEW: UIFont can now specify a UIAtlas/sprite combo instead of explicitly defining the material and pixel rect.

1.24
- NEW: Added character and line spacing parameters to UIFont.
- NEW: Sprites will now be sorted alphabetically, both on import and in the drop-down menu.
- NEW: NGUI menu Add* functions now automatically choose last used font and UI atlases and resize the new elements.
- FIX: UICamera will now be able to handle both mouse and touch-based input on non-mobile devices.
- FIX: 'Add Collider' menu option got semi-broken in 1.23.
- FIX: Changing the font will now correctly update the visible text while in the editor.
- Work-around for a bug in 3.5b6 Flash export.

1.23
- NEW: Added a pivot property to the widget class, replacing the old 'centered' flag.

1.22
- NEW: Example 6: Draggable Window
- FIX: UISlider will now function properly for arbitrarily scaled objects.

1.21
- FIX: Gizmos are now rotated properly, matching the widget's rotation.
- FIX: Labels now have gizmos properly scaled to envelop their entire content.

1.20
- NEW: Added the ability to generate normals and tangents for all widgets via a flag on UIPanel.
- NEW: Added a UITexture class that can be used to draw a texture without having to create an atlas.
- NEW: Example 5: Lights and Refraction.
- Moved all atlases into the Examples folder.

1.12
- FIX: Unicode fonts should now get imported correctly.

1.11
- NEW: Added a new example (4) - Chat Window.

1.10
- NEW: Added support for Unity 3.5 and its "export to Flash" feature.

1.09
- NEW: Added password fields (specified on the label)
- FIX: Working directly with atlas and font prefabs will now save their data correctly
- NEW: Showing gizmos is now an option specified on the panel
- NEW: Sprite inner rects will now be preserved on re-import
- FIX: Disabled widgets should no longer remain visible in play mode
- NEW: UICamera.lastHit will always contain the last RaycastHit made prior to sending OnClick, OnHover, and other events.

1.08
- NEW: Added support for multi-touch