# CopySources

���i�A���W�b�N�̑啔����DLL�����Ă���Unity�Ɏ�荞��ł�����̂́A�f�o�b�O��A���X�\�[�X�R�[�h��Unity���ɑS�R�s�[���Ď��s�������ꍇ������B

## �Q�l: CopyDllsAfterBuild�̃t���[

CopyDllsAfterBuild �ł́Acsproj �́u�r���h��C�x���g (Post-Build event command line)�v�ŁA�r���h���ʂ�DLL�ꎮ��Unity�z���ɃR�s�[����B

DLL������Ӌ`�́A

- ���܂�Ƀ\�[�X�R�[�h��������Unity�̃r���h���x���Ă炢
- �v���W�F�N�g�𕪂���Ƃ����T�O��Unity�ɂȂ��̂ŁA�K�͂��傫���Ȃ������Ɉˑ����Ǘ����炢
- Unity�̊O���瓯���R�[�h���g�������Ƃ��ɍ���
  - �f�[�^�ҏW�c�[���Ƃ��A�T�[�o�[���Ƃ̃R�[�h���L�Ƃ�

DLL�������ꍇ�̖��

- �X�^�b�N�g���[�X��ǂ��Â炢
  - �ꉞ�A.mdb �t�@�C���𐶐����Ă����΁A��O�������̃X�^�b�N�g���[�X�͉��Ƃ��ǂ���
    - �u���C�N �|�C���g�d�|������A�X�e�b�v���s��������\
  - ����ł��AProfiler �Ƃ��AAndroid ���@���s���̃g���[�X�ł͒ǂ��Ȃ��Ȃ�

## ����CopySources�̃t���[

DLL�������ꍇ�̖�������邽�߁A�K�v�ɉ����āA�\�[�X�R�[�h��S�R�s�[���Ďg���z��̃v���O�������������Ƃɂ����B
�g�������͂��������悭�āAProfiler����@�f�o�b�O�ŃX�^�b�N�g���[�X����������ǂ������ꍇ�����g���z��B

- ���i: CopyDllsAfterBuild ���g����DLL���ς݂̃R�[�h��Unity�Ɏ�荞��
- Profiler����@�f�o�b�O�ŃX�^�b�N�g���[�X���ق����Ȃ����Ƃ�: CopySources ���g���ă\�[�X�R�[�h��Unity�Ɏ�荞��

## ����

CopySources���g���đS�R�s�[�������܂�����ɁA���ɖ߂�(�R�s�[�����\�[�X�R�[�h�������āADLL�𕜌�����)����͂ł��Ȃ��B

�Ȃ̂ŁACopySources�����s����O�ɃR�~�b�g��Y�ꂸ�ɁB���ɖ߂������ꍇ�́u�ύX��j���v�������̓��Z�b�g�ŁB

���ƁAUnity���ALibrary, temp, obj �ӂ�������Ȃ��Ɨ�����B
