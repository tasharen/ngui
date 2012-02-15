----------------------------------------------
            NGUI: Next-Gen UI kit
 Copyright © 2011-2012 Tasharen Entertainment
                Version 1.65
    http://www.tasharen.com/?page_id=197
            support@tasharen.com
----------------------------------------------

Thank you for buying NGUI!

If you have any questions, suggestions, comments or feature requests, please don't hesitate to
email support@tasharen.com, PM 'ArenMook' on the Unity forums, or add 'arenmook' to Skype.

--------------------
 How To Update NGUI
--------------------

If you have the Professional Edition of NGUI that comes with SVN access, simply SVN Update.

If you have a regular copy of NGUI:

1. Create a brand new Unity project.
2. Import the latest NGUI package into this project.
3. Delete the NGUI folder and all of its contents from your old project using Explorer (on Windows) or Finder (if you’re on the Mac).
4. Copy the NGUI folder from the new project into the old project.
5. Open your old project with Unity - it should now be up to date.

------------------------------------
****** !!! IMPORTANT NOTE !!! ******
------------------------------------

If you are importing NGUI from a unity package, you will need to perform the following step
in order to get the 2D and 3D layer names to show up in-game:

If it's a clean project, simply extract the contents of the LibraryAssets.zip file into the Library
folder (Unity 3.4) or the ProjectSettings folder (Unity 3.5+), overwriting what's there.
   
If you are not importing examples, you can ignore this part.

---------------------------------------
 Support, documentation, and tutorials
---------------------------------------

All can be found here: http://www.tasharen.com/?page_id=197

-----------------
 Version History
-----------------

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