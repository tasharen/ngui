----------------------------------------
        NGUI: Next-Gen UI kit
 Copyright © 2011 Tasharen Entertainment
             Version 1.10c
  http://www.tasharen.com/?page_id=140
        support@tasharen.com
----------------------------------------

Thank you for buying NGUI!

If you have any questions, suggestions, comments or feature requests, please don't hesitate to
email support@tasharen.com, PM 'ArenMook' on the Unity forums, or add 'arenmook' to Skype.

------------------------------------
****** !!! IMPORTANT NOTE !!! ******
------------------------------------

If you are importing examples, since Unity project settings don't seem to get included in the package, you have to
do either of the two steps in order to get some examples to work properly:

1. If it's a clean project, simply extract the contents of the LibraryAssets.zip file
   into the Library folder (Unity 3.4) or the ProjectSettings folder (unity 3.5+), overwriting what's there.
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