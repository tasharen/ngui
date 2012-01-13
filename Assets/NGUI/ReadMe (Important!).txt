----------------------------------------------
            NGUI: Next-Gen UI kit
 Copyright © 2011-2012 Tasharen Entertainment
                Version 1.30f
    http://www.tasharen.com/?page_id=140
            support@tasharen.com
----------------------------------------------

Thank you for buying NGUI!

If you have any questions, suggestions, comments or feature requests, please don't hesitate to
email support@tasharen.com, PM 'ArenMook' on the Unity forums, or add 'arenmook' to Skype.

------------------------------------
****** !!! IMPORTANT NOTE !!! ******
------------------------------------

If you are importing examples, since Unity project settings don't seem to get included in the package, you have to
do either of the two steps in order to get some examples to work properly:

1. If it's a clean project, simply extract the contents of the LibraryAssets.zip file
   into the Library folder (Unity 3.4) or the ProjectSettings folder (Unity 3.5+), overwriting what's there.
2. Alternatively, or if it's an existing project, add 2 new layers: "2D UI" and "3D UI", without quotes.
   Not sure how? Select any game object, top right has a drop-down called "Layer", and it's likely set to
   "Default". Expand it, click "Add Layer", and add the 2 layers by typing them in the User Layer 8 and 9 fields.
   
If you are not importing examples, you can ignore this part.

-------------------------------------
Support, documentation, and tutorials
-------------------------------------

All can be found here: http://www.tasharen.com/?page_id=140

---------------
Version History
---------------

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